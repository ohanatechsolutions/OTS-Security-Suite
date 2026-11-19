"""Regression contracts for the *current* upstream-tool hub Windows package.

Previous RepoScan/W.H.I.T.E./Sanitizer UI assertions are historical and do not
apply to a product that intentionally excludes those executables. Preserve
archived source; verify that no historical engine is shipped.
"""

import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
HUB = ROOT / "src" / "OTSSecuritySuite.Hub"
INSTALLER = ROOT / "installer" / "OTS-Security-Suite.iss"
WORKFLOW = ROOT / ".github" / "workflows" / "windows-build.yml"


class HubReleaseManifestTests(unittest.TestCase):
    def test_original_ots_brand_asset_paths(self):
        project = (HUB / "OTSSecuritySuite.Hub.csproj").read_text(
            encoding="utf-8"
        )
        main = (HUB / "MainWindow.xaml").read_text(encoding="utf-8")
        self.assertIn(
            "<ApplicationIcon>../OTSSecuritySuite.Desktop/AppIcon.ico",
            project,
        )
        self.assertIn('Icon="AppIcon.ico"', main)
        self.assertTrue(
            (ROOT / "src/OTSSecuritySuite.Desktop/AppIcon.ico").is_file()
        )
        self.assertTrue(
            (ROOT / "src/OTSSecuritySuite.Desktop/OhanaLogo.png").is_file()
        )

    def test_hub_markup_is_well_formed(self):
        # Full WPF/XAML compilation is executed on the Windows runner.
        for path in HUB.glob("*.xaml"):
            source = path.read_text(encoding="utf-8")
            self.assertIn("x:Class=", source)
            self.assertIn("schemas.microsoft.com/winfx/2006/xaml", source)

    def test_taskbar_identity_matches_shortcuts(self):
        app = (HUB / "App.xaml.cs").read_text(encoding="utf-8")
        installer = INSTALLER.read_text(encoding="utf-8")
        app_id = "OhanaTechSolutions.OTSSecuritySuite"
        self.assertIn("SetCurrentProcessExplicitAppUserModelID", app)
        self.assertIn(app_id, app)
        self.assertEqual(
            installer.count(f'AppUserModelID: "{app_id}"'), 2
        )

    def test_only_the_upstream_hub_is_built_for_shipping(self):
        ci = WORKFLOW.read_text(encoding="utf-8")
        self.assertIn("OTSSecuritySuite.Hub.csproj", ci)
        self.assertNotIn("OTSSecuritySuite.Desktop.csproj", ci)
        self.assertNotIn("PyInstaller", ci)
        self.assertNotIn("go build", ci)
        self.assertNotIn("gh release create", ci)
        self.assertIn("Upload unsigned hub preview", ci)
        self.assertIn("no-OTS-engines", ci)

    def test_no_legacy_application_modules_are_shipped(self):
        installer = INSTALLER.read_text(encoding="utf-8")
        for excluded in ("\\engines", "\\extension", "OTSWhiteAudit.exe",
                         "OTSScannerEngine.exe", "OTSSystemSanitizer.exe"):
            self.assertIn(excluded, installer)
        self.assertNotIn("OTS-Firewall79X-MIT.txt", installer)
        self.assertIn("Flags: deletekey", installer)
        for protected in ("Quarantine", "DPAPI", "UserProfile", "Documents"):
            self.assertNotIn(protected, installer)

    def test_previous_security_source_is_preserved(self):
        self.assertTrue(
            (ROOT / "scanner/internal/engine/engine.go").is_file()
        )
        self.assertTrue(
            (ROOT / "white_engine/ots_white/cli.py").is_file()
        )
        self.assertTrue(
            (ROOT / "modules/OTS_System_Sanitizer/LICENSE").is_file()
        )
        self.assertTrue(
            (ROOT / "docs/SANITIZER_SOURCE_MANIFEST.tsv").is_file()
        )

    def test_hub_publisher_version_and_per_user_install(self):
        installer = INSTALLER.read_text(encoding="utf-8")
        self.assertIn('#define ProductVersion "0.10.0"', installer)
        self.assertIn("PrivilegesRequired=lowest", installer)
        self.assertIn("AppVersion={#ProductVersion}", installer)
        self.assertIn("CloseApplications=yes", installer)


if __name__ == "__main__":
    unittest.main()
