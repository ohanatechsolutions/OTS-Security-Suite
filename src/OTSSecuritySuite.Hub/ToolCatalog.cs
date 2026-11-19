using System.IO;

namespace OTSSecuritySuite.Hub;

public enum ToolMode { WindowsSecurity, NativeGui, CommandLine, DocumentationOnly }

public sealed record ToolSpec(
    string Id, string Name, string Category, string Purpose,
    ToolMode Mode, string OfficialUrl, string[] ExecutableNames, string SafetyNote);

/// <summary>
/// Explicitly approved external tools only. No OTS-built scanning, cleaning or firewall engine.
/// An executable is NEVER bundled, fetched or installed from these records.
/// </summary>
public static class ToolCatalog
{
    public static IReadOnlyList<ToolSpec> All { get; } = Array.AsReadOnly(new[]
    {
        new ToolSpec("defender", "Microsoft Defender", "Endpoint Protection",
            "Windows' existing antivirus protection. Open its genuine Windows Security interface; do not replace other registered antivirus products.",
            ToolMode.WindowsSecurity, "https://support.microsoft.com/windows/stay-protected-with-windows-security",
            Array.Empty<string>(), "Windows Security is managed by Windows. The hub never changes antivirus configuration."),
        new ToolSpec("clamav", "ClamAV", "Endpoint Protection",
            "Open-source antivirus engine for manual file investigation, separate from any active real-time antivirus.",
            ToolMode.CommandLine, "https://www.clamav.net/downloads",
            new[] {"clamscan.exe"}, "Help display only; no files are scanned or quarantined automatically."),
        new ToolSpec("yarax", "YARA-X", "Endpoint Protection",
            "Open-source pattern/rule matching for authorized file investigations.",
            ToolMode.CommandLine, "https://github.com/VirusTotal/yara-x",
            new[] {"yr.exe"}, "YARA rules and scan targets are chosen in the upstream tool, not silently by this hub."),
        new ToolSpec("osquery", "osquery", "Endpoint Visibility",
            "Query operating-system state with the upstream SQL-style tooling.",
            ToolMode.CommandLine, "https://osquery.io/",
            new[] {"osqueryi.exe"}, "This hub does not install an osquery service or run queries in the background."),
        new ToolSpec("kopia", "KopiaUI", "Backup & Recovery",
            "Launch Kopia's original graphical backup and restore application.",
            ToolMode.NativeGui, "https://kopia.io/docs/installation/",
            new[] {"KopiaUI.exe"}, "Backups, restores, encryption keys and repositories remain managed by Kopia."),
        new ToolSpec("hayabusa", "Hayabusa", "Incident Investigation",
            "Windows event-log threat hunting using the upstream standalone analyzer.",
            ToolMode.CommandLine, "https://github.com/Yamato-Security/hayabusa",
            new[] {"hayabusa.exe"}, "Running a hunt requires separate user-supplied logs and rule choices."),
        new ToolSpec("hardeningkitty", "HardeningKitty", "Hardening & Telemetry",
            "Windows hardening audit and configuration scripts provided by the upstream project.",
            ToolMode.DocumentationOnly, "https://github.com/scipag/HardeningKitty",
            Array.Empty<string>(), "No PowerShell policy changes or elevated scripts are run from this hub."),
        new ToolSpec("sysmon", "Microsoft Sysmon", "Hardening & Telemetry",
            "Microsoft Sysinternals host telemetry and event logging.",
            ToolMode.DocumentationOnly, "https://learn.microsoft.com/sysinternals/downloads/sysmon",
            Array.Empty<string>(), "The hub does not install drivers, create services, or change Sysmon configuration."),
        new ToolSpec("pesieve", "PE-sieve", "Incident Investigation",
            "Inspect memory for suspicious code implants using the upstream utility.",
            ToolMode.CommandLine, "https://github.com/hasherezade/pe-sieve",
            new[] {"pe-sieve.exe"}, "Live process-memory investigations are never launched automatically."),
        new ToolSpec("trufflehog", "TruffleHog", "Code & Supply Chain",
            "Identify exposed credentials in authorized source repositories.",
            ToolMode.CommandLine, "https://github.com/trufflesecurity/trufflehog",
            new[] {"trufflehog.exe"}, "No repository content or detected secrets are uploaded by the hub."),
        new ToolSpec("osvscanner", "OSV-Scanner", "Code & Supply Chain",
            "Open-source dependency vulnerability analysis using OSV data.",
            ToolMode.CommandLine, "https://github.com/google/osv-scanner",
            new[] {"osv-scanner.exe"}, "This hub only displays local executable help and never initiates network scans."),
        new ToolSpec("semgrep", "Semgrep Community Edition", "Code & Supply Chain",
            "Free community static analysis; upstream rule and redistribution terms apply.",
            ToolMode.CommandLine, "https://semgrep.dev/products/community-edition/",
            new[] {"semgrep.exe"}, "No hosted Semgrep account or paid plan is required by this application."),
        new ToolSpec("skillspector", "NVIDIA SkillSpector", "AI & Agent Security",
            "Review externally sourced agent skills for unsafe instructions and supply-chain risks.",
            ToolMode.CommandLine, "https://github.com/NVIDIA/SkillSpector",
            new[] {"skillspector.exe"}, "Only locally installed CLI help is offered; Windows support, model backend and optional external providers are managed by upstream, not OTS."),
        new ToolSpec("strix", "Strix", "Authorized Penetration Testing",
            "Optional autonomous penetration-testing tool installed and controlled independently.",
            ToolMode.CommandLine, "https://github.com/usestrix/strix",
            new[] {"strix.exe"}, "Only the local Strix --help command is available in this hub; no agent assessment, Docker container, model download or network target is started.")
    });

    public static bool ValidExecutable(ToolSpec tool, string path)
    {
        if (tool.Mode is not (ToolMode.NativeGui or ToolMode.CommandLine) ||
            string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path))
            return false;
        try
        {
            string full = Path.GetFullPath(path);
            if (!tool.ExecutableNames.Any(name =>
                    string.Equals(name, Path.GetFileName(full), StringComparison.OrdinalIgnoreCase))
                || !File.Exists(full))
                return false;

            // A filename or .exe extension does not prove this is a Windows program.
            // Read only the DOS and NT signatures; this is NOT a trust/signature verdict.
            using var stream = File.Open(full, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            if (stream.Length < 68) return false;
            Span<byte> dosHeader = stackalloc byte[64];
            stream.ReadExactly(dosHeader);
            if (dosHeader[0] != 0x4D || dosHeader[1] != 0x5A) return false;
            int peOffset = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(
                dosHeader.Slice(0x3C, 4));
            if (peOffset < 64 || peOffset > stream.Length - 4) return false;
            stream.Position = peOffset;
            Span<byte> peHeader = stackalloc byte[4];
            stream.ReadExactly(peHeader);
            return peHeader[0] == 0x50 && peHeader[1] == 0x45 &&
                peHeader[2] == 0 && peHeader[3] == 0;
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return false;
        }
    }

    public static string? FindOnPath(ToolSpec tool)
    {
        if (tool.Mode is not (ToolMode.NativeGui or ToolMode.CommandLine))
            return null;
        var folders = (Environment.GetEnvironmentVariable("PATH") ?? "")
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var directory in folders)
        {
            foreach (var binary in tool.ExecutableNames)
            {
                try
                {
                    string full = Path.GetFullPath(Path.Combine(directory.Trim('"'), binary));
                    if (ValidExecutable(tool, full)) return full;
                }
                catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException or NotSupportedException) { }
            }
        }
        return null;
    }
}
