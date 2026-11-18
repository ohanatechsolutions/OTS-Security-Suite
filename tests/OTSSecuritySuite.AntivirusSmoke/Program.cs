using OTSSecuritySuite.Desktop;

int assertions = 0;
void Check(bool valid, string name)
{
    assertions++;
    if (!valid) throw new Exception("FAILED: " + name);
}
RegisteredAntivirus Av(string name, AntivirusRuntimeState state, bool current = true)
    => new(name, state, current);
AntivirusAssessment Assess(bool available, params RegisteredAntivirus[] products)
    => AntivirusAssessment.Evaluate(products, available);
Check(Assess(false).Coverage == AntivirusCoverage.Unknown, "Unavailable inventory must not claim no AV");
Check(Assess(true).Coverage == AntivirusCoverage.Unprotected, "Empty authoritative inventory is unprotected");
Check(Assess(true, Av("Norton", AntivirusRuntimeState.Off)).Coverage == AntivirusCoverage.Unprotected, "Installed but off");
Check(Assess(true, Av("AVG", AntivirusRuntimeState.Snoozed)).Coverage == AntivirusCoverage.Unprotected, "Installed but snoozed");
Check(Assess(true, Av("ESET", AntivirusRuntimeState.Expired)).Coverage == AntivirusCoverage.Unprotected, "Expired");
Check(Assess(true, Av("Microsoft Defender Antivirus", AntivirusRuntimeState.On)).Coverage == AntivirusCoverage.Protected, "Active Defender");
Check(Assess(true, Av("Norton 360", AntivirusRuntimeState.On)).Coverage == AntivirusCoverage.Protected, "Active third-party AV");
Check(Assess(true, Av("Norton", AntivirusRuntimeState.On, false)).Coverage == AntivirusCoverage.NeedsAttention, "Stale signatures");
Check(Assess(true, Av("Norton", AntivirusRuntimeState.Unknown)).Coverage == AntivirusCoverage.Unknown, "Unknown provider state");
Check(Assess(true, Av("Defender", AntivirusRuntimeState.Off), Av("AVG", AntivirusRuntimeState.On)).Coverage == AntivirusCoverage.Protected, "Active third-party counts even when Defender passive");
Check(!Assess(true, Av("AVG", AntivirusRuntimeState.On)).ActiveDefender, "Never request Defender when third-party active");
Check(Assess(true, Av("Windows Defender Antivirus", AntivirusRuntimeState.On)).ActiveDefender, "Legacy Defender naming");
Check(!Assess(true).ActiveDefender, "Empty inventory has no active Defender");
Check(Assess(true).Guidance.Contains("built", StringComparison.OrdinalIgnoreCase), "Built-in guidance, no download");
Check(Assess(true, Av("Norton", AntivirusRuntimeState.Off)).Summary.Contains("UNPROTECTED"), "Off must display UNPROTECTED");
Check(Assess(false, Av("Norton", AntivirusRuntimeState.On)).Coverage == AntivirusCoverage.Unknown, "Partial inventory not authoritative");
Console.WriteLine($"PASS: {assertions} Windows antivirus status assertions");
