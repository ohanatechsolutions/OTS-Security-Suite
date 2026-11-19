using OTSSecuritySuite.Hub;

static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException("HubSmoke FAIL: " + message);
}

var tools = ToolCatalog.All;
Check(tools.Count == 14, "14 approved upstream tools");
Check(tools.Select(t => t.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count() == tools.Count,
    "tool IDs must be unique");
Check(tools.All(t => Uri.TryCreate(t.OfficialUrl, UriKind.Absolute, out var url) &&
                     url.Scheme == Uri.UriSchemeHttps), "official links must use HTTPS");
Check(tools.All(t => t.Mode is ToolMode.DocumentationOnly or ToolMode.WindowsSecurity ||
                      t.ExecutableNames.Length > 0), "each launchable tool has an allowed executable");

var kopia = tools.Single(t => t.Id == "kopia");
var strix = tools.Single(t => t.Id == "strix");
var skill = tools.Single(t => t.Id == "skillspector");
var defender = tools.Single(t => t.Id == "defender");
Check(kopia.Mode == ToolMode.NativeGui, "Kopia uses real native UI");
Check(strix.Mode == ToolMode.CommandLine, "Strix CLI help is available only by explicit choice");
Check(skill.Mode == ToolMode.CommandLine, "SkillSpector CLI help is available");
Check(!ToolCatalog.ValidExecutable(defender, @"C:\Windows\System32\not.exe"),
    "Windows Security cannot accept a substituted executable");
Check(!ToolCatalog.ValidExecutable(kopia, "relative.exe"), "relative paths rejected");
Check(!ToolCatalog.ValidExecutable(kopia, @"C:\does-not-exist\KopiaUI.exe"),
    "missing binaries rejected");

string root = Path.Combine(Path.GetTempPath(), "ots-hub-smoke-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
string exe = Path.Combine(root, "KopiaUI.exe");
string wrong = Path.Combine(root, "random.exe");
string priorPath = Environment.GetEnvironmentVariable("PATH") ?? "";
try
{
    File.WriteAllBytes(exe, new byte[] {0x4D, 0x5A});
    Check(!ToolCatalog.ValidExecutable(kopia, exe), "truncated MZ is not a PE executable");

    var pe = new byte[128];
    pe[0] = 0x4D; pe[1] = 0x5A;
    pe[0x3C] = 0x40; // NT header file offset
    pe[0x40] = 0x50; pe[0x41] = 0x45;
    File.WriteAllBytes(exe, pe);
    File.WriteAllBytes(wrong, pe);
    Check(ToolCatalog.ValidExecutable(kopia, exe), "proper PE header and allowlisted filename accepted");
    Check(!ToolCatalog.ValidExecutable(kopia, wrong), "unrecognized binary name rejected");
    Check(!ToolCatalog.ValidExecutable(strix, exe), "one vendor binary cannot impersonate another");

    pe[0x41] = 0x58;
    File.WriteAllBytes(exe, pe);
    Check(!ToolCatalog.ValidExecutable(kopia, exe), "invalid NT signature rejected");
    pe[0x41] = 0x45;
    File.WriteAllBytes(exe, pe);

    Environment.SetEnvironmentVariable("PATH", root);
    Check(string.Equals(ToolCatalog.FindOnPath(kopia), exe, StringComparison.OrdinalIgnoreCase),
        "path discovery connects only an explicitly named valid Windows executable");
}
finally
{
    Environment.SetEnvironmentVariable("PATH", priorPath);
    Directory.Delete(root, recursive: true);
}

Console.WriteLine("PASS: 14 upstream tools, HTTPS, non-executing vendor path policy, strict PE/NT signatures, launch modes.");
