# OTS Security Suite v0.9.0 — draft prerelease test handoff

**Purpose:** Give the repository owner an actual unsigned Windows installer for
isolated testing without merging the draft PR into main or publishing an
unreviewed binary to the public.

## Artifact location

Open [GitHub Releases](https://github.com/ohanatechsolutions/OTS_Security_Suite/releases)
while signed into the **ohanatechsolutions** account. The **DRAFT**
v0.9.0-test.1 prerelease is visible to repository collaborators with
appropriate permissions, not the general public. Its assets are:

- OTS-Security-Suite-Setup-x64.exe — genuine x64-targeted Inno Setup installer
- SHA256SUMS.txt — checksum from the same successful Windows build

The release workflow verifies Windows install/repair/uninstall, GUI automation,
Go/WHITE/Sanitizer tests and SHA-256 before creating the draft release.
The release source commit and CI run are recorded in its notes. Do not reuse
a checksum from an older run or build.

## How to test

1. Download both assets from the draft prerelease.
2. In Windows PowerShell run:
   Get-FileHash -Algorithm SHA256 .\OTS-Security-Suite-Setup-x64.exe
   Compare the 64-digit hash with SHA256SUMS.txt.
3. Install as a standard user on a disposable Windows 10/11 x64 test VM
   or spare computer, preferably with a clean Chrome test profile.
4. Confirm the logo/title/taskbar icon, DPI at 100/150/200%, real WPF launch,
   all three engines with synthetic harmless files, Cancel, report export,
   Chrome extension handoff and no unexpected network access.
5. Back up quarantine/DPAPI user profile data before attempting historical
   vault restore. Never use actual malware or production credentials.
6. Check repair/uninstall and confirm quarantine/report history survives.
7. Record PASS/FAIL, OS build, SHA, notes and sanitized screenshots in
   docs/QA_ACCEPTANCE.md or a separate issue.

**Warning:** This installer is not digitally signed. A Windows SmartScreen
warning is possible. Do not disable Defender or bypass organization policies
to install it. Its checksum alone does not authenticate Ohana Tech Solutions.

**Release status:** Draft preview only. Public release is NO-GO until
independent Windows/Chrome/security QA, legacy DPAPI acceptance, root license
decision, legitimate signing and owner release approvals are completed.

**N/A for this preview:** app stores, SaaS infrastructure, payment systems,
mobile/firmware deployment. This is an offline-first Windows desktop test.

## 2026-11-15 WHITE migration note

The original OTS W.H.I.T.E. source was **not** embedded in the suite and its
standalone repository was deleted by the owner. The new preview includes a
**replacement heuristic auditor** with selected secrets, SAST and reachable
Git-history checks; it is **not** an exact clone. Offline CVE/SCA is disabled
and unavailable. Treat the absence of findings as a limited check, not a
complete safety verdict. The original System Sanitizer submodule remains pinned.
