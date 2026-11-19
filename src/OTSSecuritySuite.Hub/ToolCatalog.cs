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
            ToolMode.DocumentationOnly, "https://github.com/NVIDIA/SkillSpector",
            Array.Empty<string>(), "Platform and local-model support must be verified upstream; no external AI credentials are used."),
        new ToolSpec("strix", "Strix", "Authorized Penetration Testing",
            "Optional autonomous penetration-testing tool installed and controlled independently.",
            ToolMode.DocumentationOnly, "https://github.com/usestrix/strix",
            Array.Empty<string>(), "No autonomous test, Docker container, model download or network target is started by this hub.")
    });

    public static bool ValidExecutable(ToolSpec tool, string path)
    {
        if (tool.Mode is not (ToolMode.NativeGui or ToolMode.CommandLine) ||
            string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path))
            return false;
        try
        {
            string full = Path.GetFullPath(path);
            return tool.ExecutableNames.Any(name =>
                string.Equals(name, Path.GetFileName(full), StringComparison.OrdinalIgnoreCase))
                && File.Exists(full);
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
