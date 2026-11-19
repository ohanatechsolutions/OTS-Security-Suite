import re
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
HUB = ROOT / "src" / "OTSSecuritySuite.Hub"

class UpstreamOnlyHubContractTests(unittest.TestCase):
    def test_hub_wpf_documents_parse(self):
        # Native WPF compilation performs full XAML parsing; do not parse
        # XML using unsafe Python stdlib parsers in a security CI job.
        for name in ("App.xaml", "MainWindow.xaml"):
            source = (HUB / name).read_text(encoding="utf8")
            self.assertIn("x:Class=", source)
            self.assertIn("schemas.microsoft.com/winfx/2006/xaml", source)

    def test_old_ots_engines_not_referenced(self):
        for name in ("ToolCatalog.cs", "MainWindow.xaml", "MainWindow.xaml.cs"):
            contents = (HUB / name).read_text(encoding="utf8")
            for obsolete in ("OTSScannerEngine", "OTSWhiteAudit", "SystemSanitizer",
                             "RepoScan", "Firewall79X", "WebView2", "localhost:"):
                self.assertNotIn(obsolete, contents, (name, obsolete))

    def test_all_approved_upstream_tools_listed_once(self):
        data = (HUB / "ToolCatalog.cs").read_text(encoding="utf8")
        names = re.findall(r'new ToolSpec\("([^"]+)"', data)
        self.assertEqual(len(names), 14)
        self.assertEqual(len(set(names)), 14)
        self.assertEqual(set(names), {
            "defender", "clamav", "yarax", "osquery", "kopia", "hayabusa",
            "hardeningkitty", "sysmon", "pesieve", "trufflehog", "osvscanner",
            "semgrep", "skillspector", "strix"})

    def test_hub_is_not_legacy_desktop(self):
        build = (ROOT / ".github" / "workflows" / "windows-build.yml").read_text(encoding="utf8")
        installer = (ROOT / "installer" / "OTS-Security-Suite.iss").read_text(encoding="utf8")
        self.assertIn("src/OTSSecuritySuite.Hub/OTSSecuritySuite.Hub.csproj", build)
        self.assertNotIn("dotnet publish src/OTSSecuritySuite.Desktop", build)
        self.assertNotIn("go build", build)
        self.assertNotIn("PyInstaller", build)
        self.assertNotIn("Source: \"..\\licenses\\OTS-Firewall79X-MIT.txt\"", installer)
        for legacy in ("\\engines", "\\extension"):
            self.assertIn(legacy, installer)
        self.assertIn('Flags: deletekey', installer)
        self.assertIn('AppVersion={#ProductVersion}', installer)

    def test_external_tools_never_auto_installed_or_scanned(self):
        source = (HUB / "MainWindow.xaml.cs").read_text(encoding="utf8")
        self.assertIn('process.StartInfo.ArgumentList.Add("--help")', source)
        self.assertNotIn('ArgumentList.Add("scan")', source)
        self.assertNotIn('winget install', source)
        self.assertNotIn('Invoke-WebRequest', source)
        self.assertIn('ToolCatalog.ValidExecutable', source)
        self.assertIn('File.Move(temp, _settingsPath, overwrite: true)', source)

    def test_pre_release_fail_closed(self):
        workflow = (ROOT / ".github" / "workflows" / "windows-build.yml").read_text(encoding="utf8")
        self.assertIn("publish-v0-10-test1:", workflow)
        self.assertIn("needs: windows-build", workflow)
        self.assertIn("OTS Universal Security", workflow)
        self.assertIn("gh release create", workflow)
        self.assertIn("--prerelease", workflow)
        self.assertIn("[ots-hub-release-v0.10.0-test.1]", workflow)
        self.assertIn("gh release download v0.9.0-test.5", workflow)
        self.assertIn("D5AA9EDB57D63E5B31B14F8FD8E130CAE696A9011087D2EF9229C15C49722A46", workflow)
        self.assertIn("preserve-legacy-vault-v0.9", workflow)
        self.assertIn("sha256sum -c SHA256SUMS.txt", workflow)
        self.assertIn("gh attestation verify", workflow)
        self.assertNotIn("gh release delete", workflow)

if __name__ == "__main__":
    unittest.main()
