import json
import subprocess
import unittest
from pathlib import Path
from xml.etree import ElementTree
ROOT = Path(__file__).resolve().parents[1]
class ReleaseManifestTests(unittest.TestCase):
    def test_windows_icon_is_source_configured(self):
        proj = (ROOT/'src/OTSSecuritySuite.Desktop/OTSSecuritySuite.Desktop.csproj').read_text(encoding="utf-8")
        xaml = (ROOT/'src/OTSSecuritySuite.Desktop/MainWindow.xaml').read_text(encoding="utf-8")
        self.assertIn('<ApplicationIcon>AppIcon.ico</ApplicationIcon>',proj)
        self.assertIn('Icon="AppIcon.ico"',xaml)
        self.assertTrue((ROOT/'src/OTSSecuritySuite.Desktop/AppIcon.ico').is_file())
        self.assertTrue((ROOT/'src/OTSSecuritySuite.Desktop/OhanaLogo.png').is_file())
    def test_xaml_parses(self):
        for f in (ROOT/'src/OTSSecuritySuite.Desktop').glob('*.xaml'):
            ElementTree.parse(f)
    def test_suite_taskbar_identity_matches_installer_shortcuts(self):
        host = (ROOT/'src/OTSSecuritySuite.Desktop/App.xaml.cs').read_text(encoding="utf-8")
        setup = (ROOT/'installer/OTS-Security-Suite.iss').read_text(encoding="utf-8")
        identifier = 'OhanaTechSolutions.OTSSecuritySuite'
        self.assertIn('SetCurrentProcessExplicitAppUserModelID', host)
        self.assertIn(identifier, host)
        self.assertEqual(setup.count('AppUserModelID: "'+identifier+'"'), 2)

    def test_extension_manifest_valid(self):
        m = json.loads((ROOT/'extension/manifest.json').read_text(encoding="utf-8"))
        self.assertEqual(m['manifest_version'],3)
        self.assertEqual(m['permissions'], ['activeTab'])
        self.assertNotIn('host_permissions', m, "Popup URL lookup requires no permanent host privileges")
        self.assertNotIn('tabs', m['permissions'], "Avoid access to unrelated browser tabs")
        self.assertNotIn('http://127.0.0.1:48728', (ROOT/'extension/popup.js').read_text(encoding="utf-8"))
    def test_antivirus_warning_and_windows_security_action_present(self):
        xaml = (ROOT/'src/OTSSecuritySuite.Desktop/MainWindow.xaml').read_text(encoding='utf-8')
        host = (ROOT/'src/OTSSecuritySuite.Desktop/MainWindow.xaml.cs').read_text(encoding='utf-8')
        model = (ROOT/'src/OTSSecuritySuite.Desktop/AntivirusProtection.cs').read_text(encoding='utf-8')
        self.assertIn('Name="ProtectionHeader"', xaml)
        self.assertIn('Click="OpenProtectionFromHeader"', xaml)
        self.assertIn('UNPROTECTED · OPEN WINDOWS SECURITY', host)
        self.assertIn('new ProcessStartInfo("windowsdefender:")', host)
        self.assertIn('WindowsAntivirusInventory.Read()', host)
        self.assertIn('AntivirusCoverage.Unknown', model)
        self.assertNotIn('Start-Service', model)
        self.assertNotIn('Set-MpPreference', model)

    def test_reposcan_replaces_distributed_white(self):
        xaml = (ROOT/'src/OTSSecuritySuite.Desktop/MainWindow.xaml').read_text(encoding='utf-8')
        host = (ROOT/'src/OTSSecuritySuite.Desktop/MainWindow.xaml.cs').read_text(encoding='utf-8')
        ci = (ROOT/'.github/workflows/windows-build.yml').read_text(encoding='utf-8')
        self.assertIn('Name="RepoTab"', xaml)
        self.assertNotIn('WhiteTab', xaml)
        self.assertNotIn('WhitePanel', xaml)
        self.assertNotIn('WhiteAuditRunner', host)
        self.assertNotIn('OTSWhiteAudit.exe', host)
        self.assertNotIn('bundle/engines/OTSWhiteAudit.exe', ci)
        self.assertIn('dist/OTSWhiteAudit.exe', ci)

    def test_sanitizer_is_no_longer_in_the_shipped_host(self):
        xaml = (ROOT/'src/OTSSecuritySuite.Desktop/MainWindow.xaml').read_text(encoding='utf-8')
        code = (ROOT/'src/OTSSecuritySuite.Desktop/MainWindow.xaml.cs').read_text(encoding='utf-8')
        project = (ROOT/'src/OTSSecuritySuite.Desktop/OTSSecuritySuite.Desktop.csproj').read_text(encoding='utf-8')
        ci = (ROOT/'.github/workflows/windows-build.yml').read_text(encoding='utf-8')
        for forbidden in ('CleanupPanel', 'CleanupTab', 'OpenSanitizer', 'QuarantineItems'):
            self.assertNotIn(forbidden, xaml)
        for forbidden in ('OTSSystemSanitizer.Core', 'CleanupService', 'ThreatScanner', 'QuarantineVaultRegistry'):
            self.assertNotIn(forbidden, code)
        self.assertNotIn('ProjectReference Include="..\\..\\modules\\OTS_System_Sanitizer', project)
        self.assertIn('Compile Remove="QuarantineVaultRegistry.cs"', project)
        self.assertNotIn('bundle/engines/OTSSystemSanitizer.exe', ci)
        self.assertNotIn('dotnet publish modules/OTS_System_Sanitizer/src/OTSSystemSanitizer.Desktop', ci)

    def test_strix_requires_explicit_local_configuration(self):
        xaml = (ROOT/'src/OTSSecuritySuite.Desktop/MainWindow.xaml').read_text(encoding='utf-8')
        code = (ROOT/'src/OTSSecuritySuite.Desktop/MainWindow.xaml.cs').read_text(encoding='utf-8')
        runner = (ROOT/'src/OTSSecuritySuite.Desktop/StrixIntegration.cs').read_text(encoding='utf-8')
        self.assertIn('Name="StrixPanel"', xaml)
        self.assertIn('Name="StrixAuthorization"', xaml)
        self.assertIn('Click="RunStrix"', xaml)
        self.assertIn('StrixSafety.Validate', code)
        self.assertIn('MessageBoxButton.YesNo', code)
        for control in ('IPAddress.IsLoopback', 'CopySnapshot', 'ProcessStartInfo', 'UseShellExecute = false',
                        'STRIX_TELEMETRY', '"--max-budget", "0"', 'CancelAfter(TimeSpan.FromMinutes(30))'):
            self.assertIn(control, runner)
        self.assertNotIn('curl -sSL', runner, 'The installer must never download or execute a remote install script')

    def test_sanitizer_retirement_tool_is_fail_closed(self):
        text = (ROOT/'tools/backup-retiring-sanitizer.ps1').read_text(encoding='utf-8')
        required = ('git clone --mirror', 'bundle create', 'bundle verify',
                    'restore-test.git', 'fsck --full', 'gh release download',
                    'Get-FileHash', 'repository_deletion_authorized = $false',
                    'original_tree', 'suite_vendored_tree',
                    'original_main', 'expectedSetupHash')
        for needle in required:
            self.assertIn(needle, text, needle)
        self.assertNotIn('gh repo delete', text)
        self.assertNotIn('git push --mirror', text)

    def test_vendored_sanitizer_source_integrity(self):
        self.assertFalse((ROOT/'.gitmodules').exists(), "A clean clone must not depend on removed upstream repos")
        self.assertTrue((ROOT/'white_engine/ots_white/cli.py').is_file())
        vendor = ROOT/'modules/OTS_System_Sanitizer'
        for relative in (
            'LICENSE',
            'THIRD_PARTY_NOTICES.md',
            'src/OTSSystemSanitizer.Core/QuarantineService.cs',
            'src/OTSSystemSanitizer.Core/OTSSystemSanitizer.Core.csproj',
            'src/OTSSystemSanitizer.Desktop/OTSSystemSanitizer.Desktop.csproj',
            'src/OTSSystemSanitizer.Desktop/Assets/OTSSystemSanitizer.ico',
            'src/OTSSystemSanitizer.Desktop/Assets/OTSSystemSanitizer.png',
            'tests/OTSSystemSanitizer.SmokeTests/Program.cs',
        ):
            self.assertTrue((vendor/relative).is_file(), relative)
        tree = subprocess.run(
            ['git', 'rev-parse', 'HEAD:modules/OTS_System_Sanitizer'],
            cwd=ROOT, check=True, capture_output=True, text=True,
        ).stdout.strip()
        self.assertEqual(tree, 'c56ef135da8b509c88d4175feba1e6055fd4b6f7')
        entries = [line for line in (ROOT/'docs/SANITIZER_SOURCE_MANIFEST.tsv').read_text(
            encoding='utf-8').splitlines() if line and not line.startswith('#')]
        self.assertEqual(len(entries), 42)
if __name__=='__main__': unittest.main()
