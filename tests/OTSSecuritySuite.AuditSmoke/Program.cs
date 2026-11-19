using System.Diagnostics;
using System.IO;
using System.Text;
using OTSSecuritySuite.Desktop;
using OTSSystemSanitizer.Core;

var cases = new (string Name, Func<Task> Action)[]
{
    ("bounded valid stdout and private stderr", ValidOutput),
    ("nonzero exit code is faithfully returned", NonzeroExit),
    ("user cancellation reaps process", Cancellation),
    ("timeout reaps process", Timeout),
    ("stdout overflow terminates process", StdoutOverflow),
    ("stderr overflow terminates process", StderrOverflow),
    ("only one concurrent audit is allowed", PreventDuplicates),
    ("invalid shell and nonredirected process rejected", RejectUnsafeConfiguration),
    ("report export is atomic and preserves old JSON on invalid input", AtomicJsonExport),
    ("report export rejects absent directory and cleans temp output", ExportFailureCleanly),
    ("quarantine registry preserves current and legacy vaults", BothQuarantineVaults),
    ("quarantine registry does not create a legacy vault", DoesNotCreateLegacyVault),
    ("WHITE JSON contract cannot report inconsistent results as clean", WhiteReportConsistency),
    ("WHITE JSON contract rejects mismatched or incomplete schemas", WhiteReportSchema),
    ("quarantine review authorization binds exact vault and hash", QuarantineReviewIdentity),
    ("only one suite per user session owns the activation pipe", SingleInstanceForwarding),
    ("native protocol rejects unsafe repository URL arguments", ProtocolRejectsUnsafeUrls),
    ("cleanup deletes only selected old temporary fixture", CleanupOnlySelected),
    ("cleanup refuses modified or recent synthetic fixtures", CleanupRejectsChangedAndRecent),
    ("cleanup rejects non-allowlisted locations and cache opt-in spoof", CleanupRejectsOutsideRoots),
    ("WHITE import accepts bounded valid JSON but does not authenticate its origin", WhiteImportValid),
    ("WHITE import rejects malformed, invalid UTF-8 and oversized files", WhiteImportRejectsUnsafe),
    ("network profile status fails closed when providers are incomplete", NetworkStatusFailsClosed),
    ("network profile status distinguishes default-allow from disabled", NetworkStatusPolicyWarnings),
    ("firewall rule hints identify duplicate and opposite rules without precedence claims", FirewallRuleOverlapHints),
    ("event history keeps Windows-owned actions distinct from detection", WindowsActivityClassification),
    ("Strix rejects nonlocal model endpoints and missing executable", StrixLocalPolicyRejectsRemote),
    ("Strix forces local model routing and removes paid keys", StrixCommandHasLocalBudget),
    ("Strix copies bounded local source without modifying originals", StrixSnapshotIsDisposable),
};
int passed = 0;
foreach (var (name, action) in cases)
{
    try
    {
        await action();
        Console.WriteLine("PASS " + name);
        passed++;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine("FAIL " + name + " : " + ex);
    }
}
Console.WriteLine($"Audit runner tests: {passed}/{cases.Length} PASS");
return passed == cases.Length ? 0 : 1;

static Task NetworkStatusFailsClosed()
{
    var missing = new[] {
        new NetworkProtectionAudit.FirewallProfile("Domain", true, true, "On"),
        new NetworkProtectionAudit.FirewallProfile("Private", null, null, "Unknown"),
        new NetworkProtectionAudit.FirewallProfile("Public", true, true, "On")
    };
    Assert(NetworkProtectionAudit.OverallStatus(missing).Contains("could not be verified", StringComparison.Ordinal),
        "Missing profile state was incorrectly reported safe");
    Assert(NetworkProtectionAudit.OverallStatus(missing.Take(2).ToArray()).Contains("could not be verified", StringComparison.Ordinal),
        "Missing profile was incorrectly reported safe");
    return Task.CompletedTask;
}

static Task NetworkStatusPolicyWarnings()
{
    var healthy = new[] {
        new NetworkProtectionAudit.FirewallProfile("Domain", true, true, "On"),
        new NetworkProtectionAudit.FirewallProfile("Private", true, true, "On"),
        new NetworkProtectionAudit.FirewallProfile("Public", true, true, "On")
    };
    Assert(NetworkProtectionAudit.OverallStatus(healthy).Contains("enabled with default inbound blocking", StringComparison.Ordinal),
        "Healthy status incorrectly reported");
    var disabled = healthy.ToArray();
    disabled[1] = disabled[1] with { Enabled = false };
    Assert(NetworkProtectionAudit.OverallStatus(disabled).Contains("disabled", StringComparison.Ordinal),
        "Disabled firewall profile not disclosed");
    var permissive = healthy.ToArray();
    permissive[2] = permissive[2] with { InboundDefaultBlock = false };
    Assert(NetworkProtectionAudit.OverallStatus(permissive).Contains("does not block inbound", StringComparison.Ordinal),
        "Permissive default inbound action not disclosed");
    return Task.CompletedTask;
}

static Task FirewallRuleOverlapHints()
{
    var allow = new FirewallRuleView("Allow A", "1", "1", "6", "*", "*", "7", "", true);
    var duplicate = allow with { Name = "Allow B" };
    var opposing = allow with { Name = "Block C", Action = "0" };
    var result = FirewallOverlap.Analyze(new[] { allow, duplicate, opposing });
    Assert(result.Examined == 3, "Not all synthetic rules were analyzed");
    Assert(result.DuplicateRules == 1, "Duplicate rules were not identified");
    Assert(result.OpposingExactMatches == 1, "Opposing action overlap was not identified");
    Assert(result.BroadInboundAllows == 2, "Broad enabled inbound allow rules were not identified");
    Assert(result.Limitations.Contains("NOT modeled", StringComparison.Ordinal),
        "Engine must disclose that effective Windows rule precedence is not modeled");
    Assert(!FirewallOverlap.PortsOverlap("invalid", "80"),
        "Invalid port syntax must not be assumed overlapping");
    return Task.CompletedTask;
}

static Task WindowsActivityClassification()
{
    var t = DateTimeOffset.UtcNow;
    var firewall = WindowsThreatActivity.Classify(5157, t, "Windows Firewall");
    Assert(firewall is { IsWindowsBlocked: true } && firewall.Outcome == "Blocked by Windows",
        "Windows Firewall block must be attributed to Windows");
    var detected = WindowsThreatActivity.Classify(1116, t, "Microsoft Defender");
    Assert(detected is { IsWindowsBlocked: false } &&
           detected.Outcome.Contains("not established", StringComparison.Ordinal),
        "Malware detection must not be mislabeled as successful remediation");
    var remediation = WindowsThreatActivity.Classify(1117, t, "Microsoft Defender");
    Assert(remediation is { IsWindowsBlocked: false } &&
           remediation.Outcome.Contains("verify result", StringComparison.Ordinal),
        "Remediation event cannot assert successful blocking");
    var audit = WindowsThreatActivity.Classify(1125, t, "Microsoft Defender");
    Assert(audit is { IsWindowsBlocked: false } &&
           audit.Outcome.Contains("not blocked", StringComparison.Ordinal),
        "Defender audit-only event cannot be called a block");
    var networkBlock = WindowsThreatActivity.Classify(1126, t, "Microsoft Defender");
    Assert(networkBlock is { IsWindowsBlocked: true } &&
           networkBlock.Outcome == "Blocked by Windows",
        "Defender network block requires Windows attribution");
    Assert(WindowsThreatActivity.Classify(9999, t, "Microsoft Defender") == null,
        "Unsupported events must not be interpreted as suspicious activity");
    Assert(WindowsThreatActivity.Classify(5157, t, "Microsoft Defender") == null,
        "Event IDs from wrong providers must not be classified");
    return Task.CompletedTask;
}

static ProcessStartInfo Powershell(string command)
{
    var psi = new ProcessStartInfo("powershell.exe")
    {
        UseShellExecute = false,
        CreateNoWindow = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };
    psi.ArgumentList.Add("-NoProfile");
    psi.ArgumentList.Add("-NonInteractive");
    psi.ArgumentList.Add("-Command");
    psi.ArgumentList.Add(command);
    return psi;
}

static async Task ValidOutput()
{
    var runner = new WhiteAuditRunner();
    var report = await runner.RunAsync(
        Powershell("[Console]::Error.WriteLine('SYNTHETIC_PRIVATE_SENTINEL'); [Console]::Out.Write('{}')"),
        TimeSpan.FromSeconds(15));
    Assert(report.ExitCode == 0, "Expected success exit code");
    Assert(report.StandardOutput.Trim() == "{}", "Expected only JSON stdout, never stderr");
    Assert(!report.StandardOutput.Contains("SYNTHETIC_PRIVATE_SENTINEL"), "Secret from stderr leaked");
    Assert(!runner.IsRunning, "Runner remained active after result");
}

static async Task NonzeroExit()
{
    var runner = new WhiteAuditRunner();
    var output = await runner.RunAsync(Powershell("[Console]::Out.Write('{}'); exit 7"), TimeSpan.FromSeconds(15));
    Assert(output.ExitCode == 7, "Unexpected nonzero exit was normalized to success");
}

static async Task Cancellation()
{
    var runner = new WhiteAuditRunner();
    var running = runner.RunAsync(Powershell("Start-Sleep -Seconds 20"), TimeSpan.FromSeconds(30));
    int pid = await WaitForPid(runner);
    runner.Cancel();
    await Expect<OperationCanceledException>(running);
    await AssertExited(pid);
    Assert(!runner.IsRunning, "Runner remained busy after cancel");
}

static async Task Timeout()
{
    var runner = new WhiteAuditRunner();
    var running = runner.RunAsync(Powershell("Start-Sleep -Seconds 20"), TimeSpan.FromMilliseconds(700));
    int pid = await WaitForPid(runner);
    await Expect<TimeoutException>(running);
    await AssertExited(pid);
}

static async Task StdoutOverflow()
{
    var runner = new WhiteAuditRunner();
    await Expect<InvalidDataException>(runner.RunAsync(
        Powershell("[Console]::Out.Write(('A' * 20000))"),
        TimeSpan.FromSeconds(15), maxStdoutCharacters: 1024));
    Assert(!runner.IsRunning, "Runner still active after stdout overflow");
}

static async Task StderrOverflow()
{
    var runner = new WhiteAuditRunner();
    await Expect<InvalidDataException>(runner.RunAsync(
        Powershell("[Console]::Error.Write(('X' * 20000)); [Console]::Out.Write('{}')"),
        TimeSpan.FromSeconds(15), maxStderrCharacters: 1024));
    Assert(!runner.IsRunning, "Runner still active after stderr overflow");
}

static async Task PreventDuplicates()
{
    var runner = new WhiteAuditRunner();
    var running = runner.RunAsync(Powershell("Start-Sleep -Seconds 20"), TimeSpan.FromSeconds(30));
    int pid = await WaitForPid(runner);
    try
    {
        await Expect<InvalidOperationException>(runner.RunAsync(Powershell("[Console]::Out.Write('{}')"),
            TimeSpan.FromSeconds(10)));
    }
    finally
    {
        runner.Cancel();
        await Expect<OperationCanceledException>(running);
        await AssertExited(pid);
    }
}

static async Task RejectUnsafeConfiguration()
{
    var runner = new WhiteAuditRunner();
    var invalid = new ProcessStartInfo("powershell.exe") { UseShellExecute = true };
    await Expect<ArgumentException>(runner.RunAsync(invalid, TimeSpan.FromSeconds(10)));
    Assert(!runner.IsRunning, "Invalid configuration left runner busy");
}

static Task AtomicJsonExport()
{
    var root = Path.Combine(Path.GetTempPath(), "ots-report-test-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(root);
    try
    {
        var file = Path.Combine(root, "report.json");
        WhiteReportWriter.Save(file, "{\"complete\":true}");
        Assert(File.ReadAllText(file) == "{\"complete\":true}", "Valid JSON was not saved");
        try
        {
            WhiteReportWriter.Save(file, "NOT JSON");
            throw new Exception("Invalid report was accepted");
        }
        catch (System.Text.Json.JsonException) { }
        Assert(File.ReadAllText(file) == "{\"complete\":true}", "Invalid JSON overwrote the existing report");
        try
        {
            WhiteReportWriter.Save(file, "[]");
            throw new Exception("Non-object JSON was accepted");
        }
        catch (InvalidDataException) { }
        Assert(Directory.GetFiles(root).Length == 1, "Atomic export leaked a temporary file");
    }
    finally { Directory.Delete(root, recursive: true); }
    return Task.CompletedTask;
}

static Task ExportFailureCleanly()
{
    var root = Path.Combine(Path.GetTempPath(), "ots-report-test-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(root);
    try
    {
        var existing = Path.Combine(root, "existing.json");
        File.WriteAllText(existing, "{\"original\":true}");
        var absent = Path.Combine(root, "missing", "report.json");
        try
        {
            WhiteReportWriter.Save(absent, "{\"new\":true}");
            throw new Exception("Write into a missing directory was accepted");
        }
        catch (DirectoryNotFoundException) { }
        Assert(File.ReadAllText(existing) == "{\"original\":true}", "Failed export damaged other report");
        Assert(Directory.GetFiles(root).Length == 1, "Failed export leaked temporary output");
    }
    finally { Directory.Delete(root, recursive: true); }
    return Task.CompletedTask;
}

static Task BothQuarantineVaults()
{
    var baseDir = Path.Combine(Path.GetTempPath(), "ots-vault-registry-" + Guid.NewGuid().ToString("N"));
    var current = Path.Combine(baseDir, "current");
    var legacy = Path.Combine(baseDir, "legacy");
    Directory.CreateDirectory(baseDir);
    Directory.CreateDirectory(legacy);
    try
    {
        var oldFile = Path.Combine(baseDir, "legacy-fixture.pdf.exe");
        var newFile = Path.Combine(baseDir, "current-fixture.pdf.exe");
        File.WriteAllText(oldFile, "SYNTHETIC LEGACY DATA");
        File.WriteAllText(newFile, "SYNTHETIC CURRENT DATA");
        var oldService = new OTSSystemSanitizer.Core.QuarantineService(legacy);
        var oldRecord = oldService.Quarantine(oldFile);
        var vaults = new QuarantineVaultRegistry(current, legacy);
        var currentRecord = vaults.Quarantine(newFile);
        Assert(File.Exists(oldRecord.VaultFile) && File.Exists(currentRecord.VaultFile),
            "Creating a new vault corrupted a previous encrypted vault");
        Assert(vaults.List().Count == 2, "Did not show both current and historical quarantine records");
        Assert(currentRecord.VaultFile.StartsWith(current, StringComparison.OrdinalIgnoreCase),
            "New quarantine must be kept in the current OTS folder");
        vaults.Restore(oldRecord);
        Assert(File.ReadAllText(oldFile) == "SYNTHETIC LEGACY DATA",
            "Legacy DPAPI/AES vault did not round-trip through the unified UI registry");
        Assert(File.Exists(currentRecord.VaultFile), "Restoring legacy record affected current vault");
        vaults.Restore(currentRecord);
        Assert(File.ReadAllText(newFile) == "SYNTHETIC CURRENT DATA",
            "Current vault data could not be restored");
        Assert(vaults.List().Count == 0, "Restored files still appear in quarantine");
    }
    finally { Directory.Delete(baseDir, recursive: true); }
    return Task.CompletedTask;
}

static Task DoesNotCreateLegacyVault()
{
    var baseDir = Path.Combine(Path.GetTempPath(), "ots-vault-registry-" + Guid.NewGuid().ToString("N"));
    var current = Path.Combine(baseDir, "new");
    var missingLegacy = Path.Combine(baseDir, "old-not-present");
    try
    {
        var vaults = new QuarantineVaultRegistry(current, missingLegacy);
        Assert(Directory.Exists(current), "Current vault was not initialized");
        Assert(!Directory.Exists(missingLegacy), "Suite must not create a fake legacy vault");
        Assert(vaults.List().Count == 0, "Empty vault reports unexpected records");
    }
    finally { if (Directory.Exists(baseDir)) Directory.Delete(baseDir, recursive: true); }
    return Task.CompletedTask;
}

static Task WhiteReportConsistency()
{
    const string good = """{"tool":"OTS W.H.I.T.E.","schema_version":"1.0","complete":true,"checks":{"secrets":"completed","sca":"skipped"},"errors":[],"findings":[{"rule_id":"TEST"}]}""";
    var valid = WhiteAuditReportValidator.Assess(good, 0);
    Assert(valid.Complete && valid.FindingCount == 1, "Valid contract must retain below-threshold findings");
    Assert(!WhiteAuditReportValidator.Assess(good, 2).Complete, "Incomplete exit code must never be labeled clean");
    var falseComplete = good.Replace("\"complete\":true", "\"complete\":false");
    Assert(!WhiteAuditReportValidator.Assess(falseComplete, 0).Complete, "Declared incomplete audit labeled clean");
    var failures = good.Replace("\"errors\":[]", "\"errors\":[\"missing local cache\"]");
    Assert(!WhiteAuditReportValidator.Assess(failures, 0).Complete, "Audit errors were ignored");
    var pending = good.Replace("\"secrets\":\"completed\"", "\"secrets\":\"pending\"");
    Assert(!WhiteAuditReportValidator.Assess(pending, 0).Complete, "Pending audit check labeled clean");
    return Task.CompletedTask;
}

static Task WhiteReportSchema()
{
    foreach (var malformed in new[] {
        "{}", "[]", """{"tool":"other","schema_version":"1.0","complete":true,"checks":{},"errors":[],"findings":[]}""",
        """{"tool":"OTS W.H.I.T.E.","schema_version":"1.0","checks":{},"errors":[],"findings":[]}"""
    }) {
        try
        {
            WhiteAuditReportValidator.Assess(malformed, 0);
            throw new Exception("Unexpected audit schema accepted");
        }
        catch (InvalidDataException) { }
    }
    return Task.CompletedTask;
}

static Task QuarantineReviewIdentity()
{
    var original = new OTSSystemSanitizer.Core.QuarantinedFile(
        Guid.NewGuid().ToString("N"), "C:\\Users\\fixture\\file.exe",
        "C:\\Users\\fixture\\current\\file.aegq",
        "AAA", DateTimeOffset.UtcNow, "synthetic-protected-key", DateTime.UtcNow, FileAttributes.Normal);
    var reviewed = QuarantineVaultRegistry.ReviewIdentity(original);
    Assert(reviewed != QuarantineVaultRegistry.ReviewIdentity(original with {
        VaultFile = "C:\\Users\\fixture\\legacy\\file.aegq"
    }), "Review of current vault authorized same ID in legacy vault");
    Assert(reviewed != QuarantineVaultRegistry.ReviewIdentity(original with { Sha256 = "BBB" }),
        "Review authorization reused after vault file hash changed");
    Assert(reviewed == QuarantineVaultRegistry.ReviewIdentity(original),
        "Review authorization key unstable for the same manifest");
    return Task.CompletedTask;
}

static async Task SingleInstanceForwarding()
{
    var scope = "CI-" + Guid.NewGuid().ToString("N");
    using var primary = new ProtocolActivation(scope);
    using var secondary = new ProtocolActivation(scope);
    Assert(primary.TryBecomePrimary(), "First suite process could not acquire instance identity");
    Assert(!secondary.TryBecomePrimary(), "Second instance incorrectly became primary");

    var received = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
    primary.StartListening(uri => received.TrySetResult(uri));
    const string action = "ots-security://scan?url=https%3A%2F%2Fgithub.com%2Fexample%2Frepo";
    Assert(await secondary.SendToPrimaryAsync(action), "Secondary protocol handoff failed");
    var message = await received.Task.WaitAsync(TimeSpan.FromSeconds(5));
    Assert(message == action, "Protocol was modified during IPC handoff");
}

static Task ProtocolRejectsUnsafeUrls()
{
    var okay = new[] {
        "ots-security://scan",
        "ots-security://scan?url=https%3A%2F%2Fgithub.com%2Fuser%2Frepo",
        "ots-security://scan?url=https%3A%2F%2Fbitbucket.org%2Fowner%2Frepo.git",
    };
    foreach (var sample in okay)
        Assert(TrustedProtocol.TryParse(sample, out _), "Rejected legitimate protocol: " + sample);

    var blocked = new[] {
        "https://github.com/user/repo",
        "ots-security://other?url=https%3A%2F%2Fgithub.com%2Fa%2Fb",
        "ots-security://scan?url=https%3A%2F%2Fgithub.com.evil.tld%2Fa%2Fb",
        "ots-security://scan?url=http%3A%2F%2Fgithub.com%2Fa%2Fb",
        "ots-security://scan?url=https%3A%2F%2Fgithub.com%2Fa%2Fb%2Ftree%2Fmain",
        "ots-security://scan?url=https%3A%2F%2Fgithub.com%2Fa%2Fb%3Ftoken%3Dx",
        "ots-security://scan?url=https%3A%2F%2Fgithub.com%2Fa%2F..%2Fb",
        "ots-security://scan?url=https%3A%2F%2Flocalhost%2Fa%2Fb",
        "ots-security://scan?url=https%3A%2F%2Fgithub.com%3A444%2Fa%2Fb",
        "ots-security://scan?url=https%3A%2F%2Fgithub.com%2Fa%2Fb&extra=1",
    };
    foreach (var sample in blocked)
        Assert(!TrustedProtocol.TryParse(sample, out _), "Accepted unsafe protocol: " + sample);
    return Task.CompletedTask;
}

static Task CleanupOnlySelected()
{
    var root=Path.Combine(Path.GetTempPath(),"OTS-Cleanup-QA-"+Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(root);
    try {
        var a=Path.Combine(root,"chosen.tmp"); var b=Path.Combine(root,"not-chosen.tmp");
        File.WriteAllText(a,"SYNTHETIC"); File.WriteAllText(b,"SYNTHETIC OTHER");
        File.SetLastWriteTimeUtc(a,DateTime.UtcNow.AddDays(-10));
        File.SetLastWriteTimeUtc(b,DateTime.UtcNow.AddDays(-10));
        var f=new FileInfo(a);
        var result=CleanupService.RemoveSelected(new [] {
            new CleanupCandidate(a,CleanupService.UserTemp,f.Length,f.LastWriteTimeUtc,false)
        });
        Assert(result.Removed==1 && result.Failures.Count==0,"Selected fixture not removed");
        Assert(!File.Exists(a) && File.Exists(b),"Cleanup removed unselected fixture");
    } finally { Directory.Delete(root,true); }
    return Task.CompletedTask;
}

static Task CleanupRejectsChangedAndRecent()
{
    var root=Path.Combine(Path.GetTempPath(),"OTS-Cleanup-QA-"+Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(root);
    try {
        var path=Path.Combine(root,"changed.tmp");
        File.WriteAllText(path,"OLD");
        File.SetLastWriteTimeUtc(path,DateTime.UtcNow.AddDays(-10));
        var info=new FileInfo(path);
        var chosen=new CleanupCandidate(path,CleanupService.UserTemp,info.Length,info.LastWriteTimeUtc,false);
        File.AppendAllText(path,"CHANGED SINCE REVIEW");
        var changed=CleanupService.RemoveSelected(new [] {chosen});
        Assert(changed.Removed==0 && changed.Failures.Count==1 && File.Exists(path),
            "Changed fixture was deleted despite denied review");
        var recent=Path.Combine(root,"recent.tmp");
        File.WriteAllText(recent,"RECENT");
        var fresh=new FileInfo(recent);
        var denied=CleanupService.RemoveSelected(new [] {
            new CleanupCandidate(recent,CleanupService.UserTemp,fresh.Length,fresh.LastWriteTimeUtc,false)
        });
        Assert(denied.Removed==0 && denied.Failures.Count==1 && File.Exists(recent),
            "Recently modified fixture was deleted");
    } finally { Directory.Delete(root,true); }
    return Task.CompletedTask;
}

static Task CleanupRejectsOutsideRoots()
{
    var root=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "OTS-Cleanup-QA-"+Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(root);
    try {
        var path=Path.Combine(root,"outside.tmp");
        File.WriteAllText(path,"SYNTHETIC PRIVATE LOCATION");
        File.SetLastWriteTimeUtc(path,DateTime.UtcNow.AddDays(-10));
        var info=new FileInfo(path);
        var result=CleanupService.RemoveSelected(new [] {
            new CleanupCandidate(path,CleanupService.UserTemp,info.Length,info.LastWriteTimeUtc,false),
            new CleanupCandidate(path,CleanupService.BrowserCache,info.Length,info.LastWriteTimeUtc,true),
        });
        Assert(result.Removed==0 && result.Failures.Count==2 && File.Exists(path),
            "Unapproved location was deleted");
    } finally { Directory.Delete(root,true); }
    return Task.CompletedTask;
}

static Task WhiteImportValid()
{
    var path = Path.Combine(Path.GetTempPath(), "ots-white-import-" + Guid.NewGuid().ToString("N") + ".json");
    const string report = "{\"tool\":\"OTS W.H.I.T.E.\",\"schema_version\":\"1.0\",\"complete\":true," +
        "\"checks\":{\"secrets\":\"completed\",\"sca\":\"skipped\"},\"errors\":[],\"findings\":[{}]}";
    try
    {
        File.WriteAllText(path, report);
        var imported = WhiteReportReader.Read(path);
        Assert(imported.Json == report, "Import modified the trusted JSON data unexpectedly");
        Assert(imported.Assessment.FindingCount == 1, "Import lost finding count");
        Assert(File.ReadAllText(path) == report, "Import modified the original report");
    }
    finally { File.Delete(path); }
    return Task.CompletedTask;
}

static Task WhiteImportRejectsUnsafe()
{
    var path = Path.Combine(Path.GetTempPath(), "ots-white-import-" + Guid.NewGuid().ToString("N") + ".json");
    try
    {
        File.WriteAllText(path, "{\"tool\":\"not white\"}");
        try { WhiteReportReader.Read(path); throw new Exception("Wrong report schema accepted"); }
        catch (InvalidDataException) { }

        File.WriteAllBytes(path, new byte[] { 0xff, 0xfe, 0x80 });
        try { WhiteReportReader.Read(path); throw new Exception("Invalid UTF-8 accepted"); }
        catch (DecoderFallbackException) { }

        using (var handle = new FileStream(path, FileMode.Open, FileAccess.Write))
            handle.SetLength(WhiteReportReader.MaxReportBytes + 1L);
        try { WhiteReportReader.Read(path); throw new Exception("Oversized report accepted"); }
        catch (InvalidDataException) { }
    }
    finally { File.Delete(path); }
    return Task.CompletedTask;
}

static Task StrixLocalPolicyRejectsRemote()
{
    var root = Path.Combine(Path.GetTempPath(), "ots-strix-qa-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(root);
    try
    {
        string cli = Path.Combine(root, "strix.exe");
        string src = Path.Combine(root, "sample");
        Directory.CreateDirectory(src);
        File.WriteAllText(cli, "SYNTHETIC TEST EXECUTABLE; NEVER RUN");
        var valid = StrixSafety.Validate(cli, src, "openai/local-test-model", "http://127.0.0.1:11434/v1");
        Assert(valid.SourceDirectory == src, "Validated source path must be unchanged");
        foreach (var endpoint in new[] {
            "https://api.openai.com/v1", "http://example.com/v1",
            "http://localhost:11434/v1", "http://127.0.0.1:11434/v1?token=x",
            "http://user:pass@127.0.0.1:11434/v1", "file:///tmp/host"
        })
        {
            try { StrixSafety.Validate(cli, src, "openai/local-test-model", endpoint);
                throw new Exception("Accepted nonlocal or credential-bearing model endpoint: " + endpoint); }
            catch (ArgumentException) { }
        }
        try { StrixSafety.Validate(Path.Combine(root, "not-present.exe"), src, "openai/local", valid.LocalEndpoint);
            throw new Exception("Accepted a missing Strix CLI"); }
        catch (ArgumentException) { }
    }
    finally { Directory.Delete(root, recursive: true); }
    return Task.CompletedTask;
}

static Task StrixCommandHasLocalBudget()
{
    var root = Path.Combine(Path.GetTempPath(), "ots-strix-qa-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(root);
    string? oldKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
    try
    {
        string cli = Path.Combine(root, "strix.exe");
        string src = Path.Combine(root, "sample");
        Directory.CreateDirectory(src);
        File.WriteAllText(cli, "SYNTHETIC; NEVER EXECUTE");
        Environment.SetEnvironmentVariable("OPENAI_API_KEY", "SYNTHETIC_PAID_KEY");
        var validated = StrixSafety.Validate(cli, src, "openai/local", "http://127.0.0.1:11434/v1");
        var info = StrixSafety.CreateStartInfo(validated, root, src, Path.Combine(root, "config.json"));
        var args = info.ArgumentList.ToArray();
        Assert(!info.UseShellExecute && info.RedirectStandardOutput && info.RedirectStandardError,
            "Strix subprocess must not go through a shell or expose stdout/stderr to the UI");
        Assert(args.Contains("-n") && args.Contains("--config") && args.Contains("--scan-mode") &&
            args.Contains("quick"), "Expected bounded, configured non-interactive run");
        int budget = Array.IndexOf(args, "--max-budget");
        Assert(budget >= 0 && args[budget + 1] == "1", "Local-only scan must have a bounded internal budget");
        Assert(!info.Environment.ContainsKey("OPENAI_API_KEY"),
            "Inherited paid-model credentials were passed to Strix");
        Assert(info.Environment["LLM_API_KEY"] == "local-only" &&
            info.Environment["LLM_API_BASE"] == validated.LocalEndpoint,
            "Strix must only receive local model connection information");
        Assert(info.Environment["STRIX_TELEMETRY"] == "0", "Telemetry must be disabled");
        Assert(info.Environment["DEDUPE_LLM_API_BASE"] == validated.LocalEndpoint, "Dedupe model must be local");
        Assert(info.Environment["STRIX_LLM_INPUT_COST_PER_MTOK"] == "0", "Local model must have zero monetary unit price");
    }
    finally
    {
        Environment.SetEnvironmentVariable("OPENAI_API_KEY", oldKey);
        Directory.Delete(root, recursive: true);
    }
    return Task.CompletedTask;
}

static Task StrixSnapshotIsDisposable()
{
    var root = Path.Combine(Path.GetTempPath(), "ots-strix-qa-" + Guid.NewGuid().ToString("N"));
    string source = Path.Combine(root, "owned");
    string copy = Path.Combine(root, "run", "source-copy");
    Directory.CreateDirectory(source);
    try
    {
        Directory.CreateDirectory(Path.Combine(source, ".git"));
        Directory.CreateDirectory(Path.Combine(source, "subdir"));
        File.WriteAllText(Path.Combine(source, ".git", "config"), "DO NOT COPY");
        File.WriteAllText(Path.Combine(source, "subdir", "app.cs"), "OWNED FIXTURE");
        try { StrixSafety.CopySnapshot(source, Path.Combine(source, "inside"), CancellationToken.None);
            throw new Exception("Accepted an overlapping Strix workspace"); }
        catch (IOException) { }
        StrixSafety.CopySnapshot(source, copy, CancellationToken.None);
        Assert(File.ReadAllText(Path.Combine(copy, "subdir", "app.cs")) == "OWNED FIXTURE",
            "Source fixture was not copied");
        Assert(!Directory.Exists(Path.Combine(copy, ".git")), "Git history/secrets were copied to Strix");
        File.WriteAllText(Path.Combine(copy, "subdir", "app.cs"), "SANDBOX MODIFICATION");
        Assert(File.ReadAllText(Path.Combine(source, "subdir", "app.cs")) == "OWNED FIXTURE",
            "Changing the Strix workspace modified the original source");
        using (var file = File.Create(Path.Combine(source, "too-large.bin")))
            file.SetLength(21L * 1024 * 1024);
        try { StrixSafety.CopySnapshot(source, Path.Combine(root, "second-copy"), CancellationToken.None);
            throw new Exception("Accepted a file over the snapshot size limit"); }
        catch (IOException) { }
    }
    finally { Directory.Delete(root, recursive: true); }
    return Task.CompletedTask;
}

static async Task<int> WaitForPid(WhiteAuditRunner runner)
{
    for (int i = 0; i < 150; i++)
    {
        if (runner.ActiveProcessId is int pid) return pid;
        await Task.Delay(20);
    }
    throw new Exception("Process did not start in 3 seconds");
}

static async Task AssertExited(int pid)
{
    for (int i = 0; i < 100; i++)
    {
        try
        {
            using var child = Process.GetProcessById(pid);
            if (child.HasExited) return;
        }
        catch (ArgumentException) { return; }
        await Task.Delay(20);
    }
    throw new Exception("Child PID " + pid + " survived termination for over two seconds");
}

static async Task Expect<T>(Task action) where T : Exception
{
    try { await action; }
    catch (T) { return; }
    throw new Exception("Expected exception " + typeof(T).Name);
}

static void Assert(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}
