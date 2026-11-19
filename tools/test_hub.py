import re
import unittest
from pathlib import Path
from xml.etree import ElementTree

ROOT = Path(__file__).resolve().parents[1]
HUB = ROOT / "src" / "OTSSecuritySuite.Hub"

class UpstreamOnlyHubContractTests(unittest.TestCase):
    def test_hub_wpf_documents_parse(self):
        for name in ("App.xaml", "MainWindow.xaml"):
            ElementTree.parse(HUB / name)

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

if __name__ == "__main__":
    unittest.main()
