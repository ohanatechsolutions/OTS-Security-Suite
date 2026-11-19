# OTS Security Suite v0.10.1-test.1 — Hardening Preview

**Unsigned Windows 10/11 x64 QA preview; not a finished 14-engine cybersecurity suite.**

This version retains the native WPF third-party-tool hub and the supported 14 upstream vendor entries. It corrects a UI race in local CLI help, improves resilience to corrupt saved tool paths and restricted settings files, and adds packaging checks for the original Ohana logo, window icon, self-contained .NET runtime and Windows version metadata.

## What's included

- Launcher for the real Windows Security interface; the genuine, separately installed KopiaUI app; and explicit local `--help` for allowlisted, separately installed upstream CLI executables.
- No proprietary OTS RepoScan, W.H.I.T.E., Sanitizer or Firewall79X scanner binaries; historical source and user vault data retained.
- Scoped 10-second, bounded-output help process. Changing selection mid-command no longer displays output under the wrong tool.
- Installer smoke including the pinned real `v0.9.0-test.5` → new hub upgrade and synthetic user-data/vault preservation, installation, launch, UI navigation and uninstall.
- CI verifies PE signature, release checksum, GitHub build provenance, packaged branding/icon/runtime and exact version, as well as independently downloaded release bytes.
- No mandatory external API credentials, paid subscriptions, automatic scanner installations or autonomous pentest operations.

## Installation and first run

1. Use a Windows 10/11 x64 test computer/VM and back up any legacy application data.
2. Download this release's `OTS-Security-Suite-Setup-x64.exe` plus `SHA256SUMS.txt`; validate SHA-256 before launching.
3. The installer is **unsigned**. Never disable Defender or SmartScreen globally to run it; review the publisher warning and the checksum.
4. Run the per-user installer, open the **OTS Security Suite** shortcut, and confirm there are 14 vendor entries.
5. Test Microsoft Windows Security without changing antivirus configuration.
6. Optionally install KopiaUI independently from its official source, then connect its `KopiaUI.exe` via the hub, open it and verify its genuine UI.
7. For a trusted upstream CLI executable installed independently, connect only its allowlisted `.exe` and use SHOW ORIGINAL CLI HELP. No security scan runs.
8. Switch tabs while help is running, test missing/wrong EXE paths, read-only settings, resizing/DPI, relaunch and uninstall. Confirm legacy vault and user data remain intact.

## Limitations & production gate

This is still a **launcher preview**, not operational integration of ClamAV, YARA-X, osquery, Hayabusa, HardeningKitty, Sysmon, PE-sieve, TruffleHog desktop scans, OSV scanning, Semgrep desktop scans, NVIDIA SkillSpector execution or Strix autonomous assessments. No trusted binaries or signatures are installed, and accepted PE header/filename is **not** proof of publisher authenticity. Physical Windows 10/11 QA, third-party license review, real customer DPAPI restore checks and Windows Authenticode code signing remain blocked until performed by the owner or qualified reviewer. Production **NO-GO**; this pre-release is for owner evaluation only.
