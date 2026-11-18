using System.Diagnostics.Eventing.Reader;
using System.Security;

namespace OTSSecuritySuite.Desktop;

/// <summary>
/// Reads existing Windows security records without changing audit policy, firewall rules,
/// antivirus settings or event-log configuration. This is not packet inspection or an IDS.
/// </summary>
public static class WindowsThreatActivity
{
    public sealed record Entry(DateTimeOffset Time, string Source, int EventId, string Summary,
        string Outcome, bool IsWindowsBlocked);
    public sealed record Snapshot(IReadOnlyList<Entry> Entries, IReadOnlyList<string> Warnings,
        DateTimeOffset CheckedAt);

    public static Entry? Classify(int eventId, DateTimeOffset time, string source)
    {
        var (summary, outcome, blocked) = (source, eventId) switch
        {
            ("Windows Firewall", 5152) => ("Network packet blocked by Windows Filtering Platform", "Blocked by Windows", true),
            ("Windows Firewall", 5157) => ("Network connection blocked by Windows Filtering Platform", "Blocked by Windows", true),
            ("Microsoft Defender", 1116) => ("Microsoft Defender malware detection", "Detected; action not established", false),
            ("Microsoft Defender", 1117) => ("Microsoft Defender remediation action recorded", "Action reported; verify result in Windows Security", false),
            ("Microsoft Defender", 1125) => ("Microsoft Defender Network Protection audit", "Observed; not blocked", false),
            ("Microsoft Defender", 1126) => ("Microsoft Defender Network Protection block", "Blocked by Windows", true),
            _ => ("", "", false)
        };
        return summary.Length == 0 ? null : new Entry(time, source, eventId, summary, outcome, blocked);
    }

    public static Snapshot Read()
    {
        var observed = new List<Entry>(200);
        var warnings = new List<string>();
        var since = DateTimeOffset.UtcNow.AddDays(-7);
        ReadLog("Security", "Windows Firewall", "*[System[(EventID=5152 or EventID=5157)]]",
            100, since, observed, warnings);
        ReadLog("Microsoft-Windows-Windows Defender/Operational", "Microsoft Defender",
            "*[System[(EventID=1116 or EventID=1117 or EventID=1125 or EventID=1126)]]",
            100, since, observed, warnings);
        return new Snapshot(observed.OrderByDescending(x => x.Time).Take(200).ToArray(),
            warnings.ToArray(), DateTimeOffset.UtcNow);
    }

    private static void ReadLog(string logName, string source, string xpath, int limit,
        DateTimeOffset since, List<Entry> output, List<string> warnings)
    {
        try
        {
            var query = new EventLogQuery(logName, PathType.LogName, xpath) { ReverseDirection = true };
            using var reader = new EventLogReader(query);
            for (var count = 0; count < limit; count++)
            {
                using var record = reader.ReadEvent();
                if (record == null) break;
                if (record.TimeCreated is not DateTime recorded) continue;
                var time = new DateTimeOffset(recorded);
                if (time < since) break;
                var result = Classify(record.Id, time, source);
                if (result != null) output.Add(result);
            }
        }
        catch (Exception ex) when (ex is EventLogException or UnauthorizedAccessException
            or SecurityException or InvalidOperationException)
        {
            warnings.Add(source + " event log unavailable (" + ex.GetType().Name +
                "). Windows may restrict log access or recording may be disabled.");
        }
    }
}
