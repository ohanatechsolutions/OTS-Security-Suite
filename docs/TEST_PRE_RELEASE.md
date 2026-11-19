> **HISTORICAL PREVIEW PROCEDURE — current scope superseded:** The owner-approved target now uses reviewed free external security engines instead of old OTS RepoScan/W.H.I.T.E./Sanitizer/Firewall79X implementations. See [TOOLING_DECISIONS.md](TOOLING_DECISIONS.md) and [IMPLEMENTATION_ROADMAP.md](IMPLEMENTATION_ROADMAP.md). Old source/test/release statements below are dated evidence, not current product scope or proof these replacement engines are integrated. The existing main-branch app still launches RepoScan; Strix is adapter-tested only; independent real Docker/Windows acceptance and new-product release remain NO-GO. Preserve historical source and original vault/backup data. Re-baseline this guide for each upstream external integration before reuse.

> **2026-11-15 — VERIFIED owner-only test prerelease:** [GitHub Windows Actions #43820859841](https://github.com/ohanatechsolutions/OTS_Security_Suite/actions/runs/43820859841) **SUCCESS** (both Windows build and draft prerelease jobs), source commit 953f1f3037cb4a2f8781c95ae8823370db5ef086. The owner-visible [DRAFT v0.9.0-test.2 prerelease](https://github.com/ohanatechsolutions/OTS_Security_Suite/releases/tag/untagged-8b649701cd0ffb13c62d) contains a **real unsigned** OTS-Security-Suite-Setup-x64.exe (102,419,492 bytes, genuine MZ PE) and SHA256SUMS.txt. The authenticated GitHub Release API independently confirms draft=true, prerelease=true, both assets and installer digest sha256:f672384e94c26c97664a368dfba98fafcb52c17b62fb0ead4a3ad11ae7841dd2. The Windows job verified Go, 95 rules, 8/8 new Python WHITE tests, 22/22 .NET tests, WPF UI automation, Inno installation, repair/uninstall and synthetic quarantine preservation. **Only a TEST PREVIEW is available; independent Windows10/11 acceptance, genuine legacy vault QA, Chrome clean-profile, old WHITE detection parity, offline SCA, code signing and public release approval remain NOT VERIFIED / NO-GO.** The older test.1 draft is superseded. Further draft release creation is manual-only, with a unique tag, to prevent unintended repeat publication or CI costs.

# OTS Security Suite v0.9.0 — draft prerelease test handoff

**Purpose:** Give the repository owner an actual unsigned Windows installer for
isolated testing without merging the draft PR into main or publishing an
unreviewed binary to the public.

## Artifact location

Open [GitHub Releases](https://github.com/ohanatechsolutions/OTS_Security_Suite/releases)
while signed into the **ohanatechsolutions** account. The **DRAFT**
v0.9.0-test.2 prerelease is visible to repository collaborators with
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

**Release-workflow correction:** The earlier test.1 draft has a valid 102,426,238-byte EXE and matching checksum, but its post-create job incorrectly queried a draft release through the tag endpoint (HTTP 404). The corrected test.2 job verifies draft assets through the authenticated release listing. Test.1 is historical; use the newer test.2 draft for testing after its own CI succeeds.
