# Windows Quickstart — OTS Security Suite Hub

**Scope:** Windows 10/11 x64, free local WPF desktop launcher. `v0.10.1-test.1` is an **unsigned owner-QA preview**, not an antivirus replacement.

## Download and install

From [GitHub Releases](https://github.com/ohanatechsolutions/OTS-Security-Suite/releases), choose the newest tagged hub **pre-release** (not `v0.9.x`, which contains obsolete OTS code). Download `OTS-Security-Suite-Setup-x64.exe` and `SHA256SUMS.txt` from the *same release*.

In PowerShell inside the download directory:

```powershell
Get-FileHash .\OTS-Security-Suite-Setup-x64.exe -Algorithm SHA256
Get-Content .\SHA256SUMS.txt
```

Compare the full hexadecimal checksum strings. Back up any existing OTS Security Suite data and legacy Sanitizer vault before updating. Keep Windows Defender and SmartScreen **enabled**. The preview is not publisher Authenticode-signed; a warning is expected. Do not run a binary if the hash differs or you cannot confirm the source. If your machine's policy prohibits unsigned software, do not bypass that policy; wait for signing.

The installer installs to the current user's local application programs folder and does not require administrative privileges or download any vendor scanner. Open **OTS Security Suite** from the Windows Start menu.

## Connect a third-party tool

The left navigation lists 14 selected *projects*, not 14 fully wired scanners. For **Microsoft Defender**, click **OPEN ORIGINAL APPLICATION** to use the genuine Windows Security UI. For **KopiaUI**, install the software independently from its official project, choose **CONNECT INSTALLED .EXE** and select `KopiaUI.exe`, then click **OPEN ORIGINAL APPLICATION**. The vendor manages backup setup and encryption. Other supported command-line programs can be connected by choosing the separately installed, trusted upstream executable, and **SHOW ORIGINAL CLI HELP** displays at most a bounded amount of output, with a 10-second limit.

**Important:** An allowlisted filename and PE header do not authenticate an executable. Only connect software downloaded and verified from an official vendor. The hub does not scan, quarantine, update, install drivers, run PowerShell hardening scripts, or execute autonomous pentests.

## Build from source (Windows)

Requires a Windows 10/11 x64 development machine with the **.NET 10 SDK** and **Inno Setup 6** for the installer. Python 3.12 is used for source-contract tests.

```powershell
python -m unittest discover -s tools -p 'test_hub.py' -v
python -m unittest discover -s tools -p 'test_package.py' -v
dotnet run --project tests/OTSSecuritySuite.HubSmoke/OTSSecuritySuite.HubSmoke.csproj -c Release
dotnet publish src/OTSSecuritySuite.Hub/OTSSecuritySuite.Hub.csproj -c Release -r win-x64 --self-contained true -o bundle
& "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe" installer/OTS-Security-Suite.iss
Get-FileHash artifacts/OTS-Security-Suite-Setup-x64.exe -Algorithm SHA256
```

Use the pinned GitHub Actions workflows for the full historical-upgrade, install/start/uninstall and release-provenance QA. The installer package is a standard Inno installer, not a single-file WPF executable with every third-party engine embedded.

## Troubleshooting

- **Tool not connected:** Install the official vendor software independently, then select its trusted executable or configure PATH. The hub never downloads the tool.
- **Executable rejected:** The name, path or PE header is not expected. Do not rename untrusted files to bypass checks.
- **Settings cannot be saved:** Verify access to `%LOCALAPPDATA%\Ohana Tech Solutions\Security Suite\`. A connection may work for the current session only.
- **CLI help timed out:** The process is terminated after 10 seconds. Some upstream programs may require environment setup; the hub does not start scans.
- **Window sizing:** Resize and reopen the app; test Windows DPI scaling and send screen details for reproducible UI defects.
- **Windows Security fails to open:** Check the Windows Security installation and applicable system policy rather than installing a second antivirus.
- **Old data:** Never delete user vaults. Keep backups until a real legacy DPAPI recovery test succeeds.

## Credentials and configuration

The **hub itself** requires **no API keys, OAuth apps, `.env`, webhook, server, database, cloud subscription or persistent background process**. Tool locations are stored per user in `hub-tool-paths.json` under local app data, not in the repository. Optional external vendor projects may have their own configuration and credentials; their setup is not performed by this hub.

## Acceptance and release scope

The current published preview is for owner QA. True per-tool scanner orchestration, offline database updates, third-party license and redistribution clearance, signed publisher identity, real-vault migration and physical Windows acceptance are **not yet verified**. They are release blockers, not silently marked N/A.
