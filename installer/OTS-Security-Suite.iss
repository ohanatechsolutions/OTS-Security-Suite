#define ProductName "OTS Security Suite"
#define ProductVersion "0.10.0"
#define PublisherName "Ohana Tech Solutions"

[Setup]
AppId={{D982EB60-E78C-48A1-98B1-B88D1CF71690}
AppName={#ProductName}
AppVersion={#ProductVersion}
AppPublisher={#PublisherName}
AppComments=Third-party cybersecurity application hub. No scanner binaries are bundled.
DefaultDirName={localappdata}\Programs\Ohana Tech Solutions\Security Suite
DefaultGroupName={#ProductName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputDir=..\artifacts
OutputBaseFilename=OTS-Security-Suite-Setup-x64
SetupIconFile=AppIcon.ico
UninstallDisplayIcon={app}\OTSSecuritySuite.exe
Compression=lzma2
SolidCompression=yes
ArchitecturesAllowed=x64compatible
WizardStyle=modern
CloseApplications=yes
RestartApplications=no

; The prior 0.9.x product bundled OTS scanners and a browser extension.
; They are obsolete for the upstream-tool hub. Remove only known
; application-managed old assets, never user profiles, vaults or reports.
[InstallDelete]
Type: filesandordirs; Name: "{app}\engines"
Type: filesandordirs; Name: "{app}\extension"
Type: files; Name: "{app}\OTSWhiteAudit.exe"
Type: files; Name: "{app}\OTSScannerEngine.exe"
Type: files; Name: "{app}\OTSSystemSanitizer.exe"

[Files]
Source: "..\bundle\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion

[Icons]
Name: "{userdesktop}\OTS Security Suite"; Filename: "{app}\OTSSecuritySuite.exe"; WorkingDir: "{app}"; IconFilename: "{app}\OTSSecuritySuite.exe"; Tasks: desktopicon; AppUserModelID: "OhanaTechSolutions.OTSSecuritySuite"
Name: "{autoprograms}\OTS Security Suite"; Filename: "{app}\OTSSecuritySuite.exe"; WorkingDir: "{app}"; IconFilename: "{app}\OTSSecuritySuite.exe"; AppUserModelID: "OhanaTechSolutions.OTSSecuritySuite"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: checkedonce

; Unregister the obsolete RepoScan/Chrome-extension protocol from 0.9.x.
; No new protocol is registered by the upstream-tool hub.
[Registry]
Root: HKCU; Subkey: "Software\Classes\ots-security"; Flags: deletekey

[Run]
Filename: "{app}\OTSSecuritySuite.exe"; Description: "Launch OTS Security Suite"; Flags: nowait postinstall skipifsilent
