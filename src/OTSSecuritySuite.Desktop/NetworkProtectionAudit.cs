using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace OTSSecuritySuite.Desktop;

/// <summary>
/// A read-only network inspection adapter inspired by the Firewall79X network inventory.
/// It never alters Defender, firewall profiles, rules, drivers, or security services.
/// </summary>
public static class NetworkProtectionAudit
{
    public sealed record FirewallProfile(string Name, bool? Enabled, bool? InboundDefaultBlock, string Detail);
    public sealed record NetworkConnection(string Local, string Remote, string State);
    public sealed record Snapshot(DateTimeOffset CheckedAt, IReadOnlyList<FirewallProfile> Profiles,
        IReadOnlyList<NetworkConnection> Connections, int TotalConnections, string? NetworkError);

    public static string OverallStatus(IReadOnlyList<FirewallProfile> profiles)
    {
        if (profiles.Count != 3 || profiles.Any(p => p.Enabled is null || p.InboundDefaultBlock is null))
            return "Windows Firewall status could not be verified. Check Windows Security.";
        if (profiles.Any(p => p.Enabled == false))
            return "Attention: Windows Firewall is disabled for at least one network profile.";
        if (profiles.Any(p => p.InboundDefaultBlock == false))
            return "Review inbound firewall policy: at least one profile does not block inbound connections by default.";
        return "Windows Firewall is enabled with default inbound blocking on all three profiles.";
    }

    public static Snapshot Read()
    {
        // This is a status probe, not a packet-capture, IDS, or policy enforcement engine.
        var profiles = ReadFirewallProfiles();
        var connections = new List<NetworkConnection>();
        var total = 0;
        string? error = null;
        try
        {
            var entries = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections();
            total = entries.Length;
            foreach (var entry in entries.OrderBy(e => e.RemoteEndPoint.Address.ToString(), StringComparer.Ordinal)
                         .ThenBy(e => e.RemoteEndPoint.Port).Take(250))
            {
                connections.Add(new NetworkConnection(
                    FormatEndpoint(entry.LocalEndPoint),
                    FormatEndpoint(entry.RemoteEndPoint),
                    entry.State.ToString()));
            }
        }
        catch (Exception ex) when (ex is NetworkInformationException or UnauthorizedAccessException
            or InvalidOperationException or System.Security.SecurityException)
        {
            error = "TCP connection inventory unavailable: " + ex.GetType().Name;
        }
        return new Snapshot(DateTimeOffset.UtcNow, profiles, connections, total, error);
    }

    private static string FormatEndpoint(IPEndPoint endpoint)
    {
        // Read-only inspection, no DNS lookup or traffic generation.
        return endpoint.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6
            ? "[" + endpoint.Address + "]:" + endpoint.Port
            : endpoint.Address + ":" + endpoint.Port;
    }

    /// <summary>
    /// Analyze at most 801 enabled Windows Firewall rules for conservative overlap hints.
    /// Advisory only: this never changes rule state and does not model effective WFP/GPO precedence.
    /// </summary>
    public static string AnalyzeRules()
    {
        object? policy = null;
        try
        {
            var type = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
            if (type == null) return "Windows Firewall rule provider unavailable.";
            policy = Activator.CreateInstance(type);
            if (policy == null) return "Windows Firewall rule provider unavailable.";
            dynamic firewall = policy;
            var selected = new List<FirewallRuleView>(801);
            foreach (object comRule in firewall.Rules)
            {
                try
                {
                    dynamic rule = comRule;
                    if (!(bool)rule.Enabled) continue;
                    selected.Add(new FirewallRuleView(
                        (string?)rule.Name ?? "",
                        ((int)rule.Direction).ToString(System.Globalization.CultureInfo.InvariantCulture),
                        ((int)rule.Action).ToString(System.Globalization.CultureInfo.InvariantCulture),
                        ((int)rule.Protocol).ToString(System.Globalization.CultureInfo.InvariantCulture),
                        (string?)rule.LocalPorts ?? "",
                        (string?)rule.RemoteAddresses ?? "",
                        ((int)rule.Profiles).ToString(System.Globalization.CultureInfo.InvariantCulture),
                        (string?)rule.ApplicationName ?? "",
                        true));
                    if (selected.Count >= 801) break;
                }
                finally
                {
                    if (Marshal.IsComObject(comRule)) Marshal.ReleaseComObject(comRule);
                }
            }
            var result = FirewallOverlap.Analyze(selected);
            return "Read-only firewall rule hints: " + result;
        }
        catch (Exception ex) when (ex is COMException or UnauthorizedAccessException
            or InvalidCastException or InvalidOperationException or System.Security.SecurityException
            or Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
        {
            return "Firewall rule analysis unavailable: " + ex.GetType().Name +
                ". No firewall settings were changed.";
        }
        finally
        {
            if (policy != null && Marshal.IsComObject(policy))
                Marshal.ReleaseComObject(policy);
        }
    }

    private static IReadOnlyList<FirewallProfile> ReadFirewallProfiles()
    {
        object? policy = null;
        try
        {
            var type = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
            if (type == null) return Unknown("Windows Firewall policy provider is not available");
            policy = Activator.CreateInstance(type);
            if (policy == null) return Unknown("Windows Firewall policy provider could not start");
            dynamic firewall = policy;
            var profiles = new List<FirewallProfile>(3);
            foreach (var (number, name) in new[] { (1, "Domain"), (2, "Private"), (4, "Public") })
            {
                bool enabled = firewall.FirewallEnabled[number];
                // NET_FW_ACTION_BLOCK = 0; this is only the default inbound policy,
                // not an assertion that all explicit allow rules or effective policy are safe.
                bool inboundDefaultBlock = (int)firewall.DefaultInboundAction[number] == 0;
                profiles.Add(new FirewallProfile(name, enabled, inboundDefaultBlock,
                    enabled ? (inboundDefaultBlock ? "On · inbound default block" : "On · inbound default allow")
                            : "Firewall off"));
            }
            return profiles;
        }
        catch (Exception ex) when (ex is COMException or UnauthorizedAccessException
            or InvalidCastException or InvalidOperationException or System.Security.SecurityException)
        {
            return Unknown("Windows Firewall status unavailable: " + ex.GetType().Name);
        }
        finally
        {
            if (policy != null && Marshal.IsComObject(policy))
                Marshal.ReleaseComObject(policy);
        }
    }

    private static IReadOnlyList<FirewallProfile> Unknown(string why) =>
        new[] { "Domain", "Private", "Public" }
            .Select(name => new FirewallProfile(name, null, null, why)).ToArray();
}
