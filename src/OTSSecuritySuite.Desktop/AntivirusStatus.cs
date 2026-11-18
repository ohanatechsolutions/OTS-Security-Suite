using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;

namespace OTSSecuritySuite.Desktop;

internal enum AntivirusProtection { Active, Inactive, NotDetected, Unknown }

internal sealed record AntivirusProduct(string Name, bool? RealTimeOn, bool? SignaturesCurrent);

internal sealed record AntivirusSnapshot(
    AntivirusProtection State,
    IReadOnlyList<AntivirusProduct> Products,
    bool DefenderAvailable,
    bool DefenderActive,
    string Detail)
{
    internal bool NeedsAttention => State is AntivirusProtection.Inactive or AntivirusProtection.NotDetected;

    internal string Summary => State switch
    {
        AntivirusProtection.Active => "REAL-TIME ANTIVIRUS REPORTED ON",
        AntivirusProtection.Inactive => "NO ACTIVE ANTIVIRUS REPORTED",
        AntivirusProtection.NotDetected => "NO ANTIVIRUS REGISTERED",
        _ => "ANTIVIRUS STATUS COULD NOT BE VERIFIED"
    };

    internal string Guidance => State switch
    {
        AntivirusProtection.Active => "Windows reports real-time antivirus enabled. This does not guarantee the system is free of threats.",
        AntivirusProtection.Inactive => "An antivirus product is registered but no active real-time protection was confirmed. Open Windows Security to review its status.",
        AntivirusProtection.NotDetected => "Windows Security Center lists no antivirus provider. Microsoft Defender Antivirus is built into Windows 10/11; open Windows Security to enable or repair protection.",
        _ => "Security-provider information is unavailable or inconclusive. Verify your status directly in Windows Security; do not assume protection is disabled."
    };
}

internal static class AntivirusStatus
{
    // Windows Security Center is the source of registered third-party provider status.
    // Defender's own status takes precedence for Defender only. No registry edits,
    // installations, settings changes, exclusions, or network access are made.
    private const string Script = """
        $ErrorActionPreference = 'Stop'
        $securityCenterAvailable = $true
        $providers = @()
        try {
          $providers = @(Get-CimInstance -Namespace 'root/SecurityCenter2' -ClassName AntiVirusProduct -ErrorAction Stop |
            Select-Object -First 32 |
            ForEach-Object { [pscustomobject]@{ Name = [string]$_.displayName; State = [int]$_.productState } })
        } catch { $securityCenterAvailable = $false }
        $defender = $null
        try {
          $mp = Get-MpComputerStatus -ErrorAction Stop
          if ($null -ne $mp) {
            $defender = [pscustomobject]@{
              Available = $true
              AntivirusEnabled = [bool]$mp.AntivirusEnabled
              RealTimeProtectionEnabled = [bool]$mp.RealTimeProtectionEnabled
              RunningMode = [string]$mp.AMRunningMode
            }
          }
        } catch { }
        [pscustomobject]@{
          SecurityCenterAvailable = $securityCenterAvailable
          Providers = @($providers)
          Defender = $defender
        } | ConvertTo-Json -Compress -Depth 5
        """;

    internal static async Task<AntivirusSnapshot> ReadAsync(CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows())
            return new(AntivirusProtection.Unknown, [], false, false, "Antivirus detection is supported only on Windows.");
        var start = new ProcessStartInfo("powershell.exe")
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        start.ArgumentList.Add("-NoProfile");
        start.ArgumentList.Add("-NonInteractive");
        start.ArgumentList.Add("-EncodedCommand");
        start.ArgumentList.Add(Convert.ToBase64String(Encoding.Unicode.GetBytes(Script)));
        try
        {
            using var process = Process.Start(start) ?? throw new InvalidOperationException("PowerShell could not be started.");
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(12));
            var outputTask = process.StandardOutput.ReadToEndAsync(timeout.Token);
            // Read stderr concurrently to prevent the process blocking on a full pipe.
            var errorTask = process.StandardError.ReadToEndAsync(timeout.Token);
            try
            {
                await process.WaitForExitAsync(timeout.Token);
                var output = await outputTask;
                _ = await errorTask; // Do not expose local system details or error streams in UI.
                if (process.ExitCode != 0 || output.Length > 65536 || string.IsNullOrWhiteSpace(output))
                    return Unknown("The Windows protection status query did not return valid results.");
                return Parse(output);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                try { if (!process.HasExited) process.Kill(entireProcessTree: true); } catch (InvalidOperationException) { }
                return Unknown("The Windows protection status query timed out.");
            }
            finally
            {
                if (!process.HasExited)
                    try { process.Kill(entireProcessTree: true); } catch (InvalidOperationException) { }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException or IOException or JsonException)
        {
            return Unknown("Windows protection status could not be queried.");
        }
    }

    internal static AntivirusSnapshot Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        if (!root.TryGetProperty("SecurityCenterAvailable", out var available) ||
            available.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
            return Unknown("Windows Security Center did not return a valid response.");
        bool wscAvailable = available.GetBoolean();
        bool defenderAvailable = false, defenderOn = false;
        if (root.TryGetProperty("Defender", out var defender) && defender.ValueKind == JsonValueKind.Object)
        {
            defenderAvailable = defender.TryGetProperty("Available", out var da) && da.ValueKind == JsonValueKind.True;
            bool antivirusEnabled = defender.TryGetProperty("AntivirusEnabled", out var av) && av.ValueKind == JsonValueKind.True;
            bool realTimeEnabled = defender.TryGetProperty("RealTimeProtectionEnabled", out var rt) && rt.ValueKind == JsonValueKind.True;
            string mode = defender.TryGetProperty("RunningMode", out var rm) && rm.ValueKind == JsonValueKind.String
                ? rm.GetString() ?? "" : "";
            // Passive, EDR block, or unknown modes are not authoritative for real-time AV.
            defenderOn = defenderAvailable && antivirusEnabled && realTimeEnabled &&
                string.Equals(mode, "Normal", StringComparison.OrdinalIgnoreCase);
        }

        var products = new List<AntivirusProduct>();
        if (root.TryGetProperty("Providers", out var entries) && entries.ValueKind == JsonValueKind.Array)
            foreach (var entry in entries.EnumerateArray())
            {
                if (entry.ValueKind != JsonValueKind.Object ||
                    !entry.TryGetProperty("Name", out var nameProperty) ||
                    nameProperty.ValueKind != JsonValueKind.String) continue;
                string name = (nameProperty.GetString() ?? "").Trim();
                if (string.IsNullOrWhiteSpace(name)) continue;
                if (name.Length > 120) name = name[..120];
                bool? active = null, definitionsCurrent = null;
                if (entry.TryGetProperty("State", out var stateProperty) &&
                    stateProperty.ValueKind == JsonValueKind.Number &&
                    stateProperty.TryGetInt32(out int rawState) && rawState is >= 0 and <= 0xFFFFFF)
                {
                    // Security Center's productState byte flags are not a formal
                    // documented compatibility contract; interpret conservatively.
                    int state = (rawState >> 8) & 0xFF;
                    active = state switch { 0x10 or 0x11 => true, 0x00 or 0x01 => false, _ => null };
                    int signatures = rawState & 0xFF;
                    definitionsCurrent = signatures switch { 0x00 => true, 0x10 => false, _ => null };
                }
                if (name.Contains("Defender", StringComparison.OrdinalIgnoreCase) && defenderAvailable)
                    active = defenderOn;
                products.Add(new AntivirusProduct(name, active, definitionsCurrent));
            }
        if (defenderAvailable && !products.Any(p => p.Name.Contains("Defender", StringComparison.OrdinalIgnoreCase)))
            products.Add(new AntivirusProduct("Microsoft Defender Antivirus", defenderOn, null));
        bool anyActive = products.Any(p => p.RealTimeOn == true) || defenderOn;
        bool anyUnknown = products.Any(p => p.RealTimeOn == null);
        AntivirusProtection status = anyActive ? AntivirusProtection.Active :
            anyUnknown || (!wscAvailable && !defenderAvailable) || (!wscAvailable && !defenderOn)
                ? AntivirusProtection.Unknown :
            products.Count == 0 ? AntivirusProtection.NotDetected : AntivirusProtection.Inactive;
        return new(status, products, defenderAvailable, defenderOn,
            wscAvailable ? "Windows Security Center queried." : "Windows Security Center unavailable; Defender queried separately where possible.");
    }

    private static AntivirusSnapshot Unknown(string message) =>
        new(AntivirusProtection.Unknown, [], false, false, message);
}
