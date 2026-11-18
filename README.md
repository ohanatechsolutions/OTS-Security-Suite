> **Windows antivirus coexistence (AV-001):** The Windows Security tab and header now show registered antivirus providers and their protection state using Windows Security Center (active, outdated, disabled, expired, unavailable). OTS never installs or switches antivirus. If no active antivirus is registered, the app directs the user to the built-in Windows Security app. [Implementation and test evidence](docs/AV_001_ANTIVIRUS_COVERAGE.md). The wider multi-engine endpoint protection plan is not yet implemented.

> **2026-11-18 source consolidation MERGED:** [PR #5](https://github.com/ohanatechsolutions/OTS_Security_Suite/pull/5) merged at `d190b03726c08cc0dfb90d62592ff0355a0f930a`. [Windows build #44301998625](https://github.com/ohanatechsolutions/OTS_Security_Suite/actions/runs/44301998625) **SUCCESS**: 42-file Sanitizer source identity, scanner/WHITE/Sanitizer tests, provenance, Inno installer and synthetic installed-app smoke passed. Old Sanitizer Git submodule removed; standard Git clone now contains the complete source tree. **This is not authorization to delete the old repository or release to the public**: archive historic Git/release assets and complete independent Windows/legacy vault QA first. The original WHITE source is unavailable and full historical parity/offline SCA remain unresolved. See [consolidation audit](docs/REPOSITORY_CONSOLIDATION_2026-11-18.md).

> **2026-11-15 — VERIFIED owner-only test prerelease:** [GitHub Windows Actions #43820859841](https://github.com/ohanatechsolutions/OTS_Security_Suite/actions/runs/43820859841) **SUCCESS** (both Windows build and draft prerelease jobs), source commit 953f1f3037cb4a2f8781c95ae8823370db5ef086. The owner-visible [DRAFT v0.9.0-test.2 prerelease](https://github.com/ohanatechsolutions/OTS_Security_Suite/releases/tag/untagged-8b649701cd0ffb13c62d) contains a **real unsigned** OTS-Security-Suite-Setup-x64.exe (102,419,492 bytes, genuine MZ PE) and SHA256SUMS.txt. The authenticated GitHub Release API independently confirms draft=true, prerelease=true, both assets and installer digest sha256:f672384e94c26c97664a368dfba98fafcb52c17b62fb0ead4a3ad11ae7841dd2. The Windows job verified Go, 95 rules, 8/8 new Python WHITE tests, 22/22 .NET tests, WPF UI automation, Inno installation, repair/uninstall and synthetic quarantine preservation. **Only a TEST PREVIEW is available; independent Windows10/11 acceptance, genuine legacy vault QA, Chrome clean-profile, old WHITE detection parity, offline SCA, code signing and public release approval remain NOT VERIFIED / NO-GO.** The older test.1 draft is superseded. Further draft release creation is manual-only, with a unique tag, to prevent unintended repeat publication or CI costs.

# OTS Security Suite — Integrated Preview 0.9.0

**Ohana Tech Solutions** · Windows 10/11 x64 · Native WPF application with offline/local scan engines

This is the first integrated development build of three OTS projects. It **is not yet approved for public release**. Source is organized for a single Windows installer; it preserves the existing component codebases and quarantine format rather than replacing them with a new unvalidated implementation.

## Latest engineering verification (unreleased)

**Last verified engineering-code commit:** `ba7562b41e7ec1abda69406fc0b1a235993e3df7` — [Windows CI #43414634076](https://github.com/ohanatechsolutions/OTS_Security_Suite/actions/runs/43414634076) **SUCCESS**, Windows runner. Actual logs: Go tests/vet and scanner build PASS; Chrome popup contracts **13/13 PASS**; embedded RepoScan URL normalization **17/17 PASS**; offline W.H.I.T.E. source/history audit **172 worktree files scanned, zero high/critical findings reported by the configured audit** (not an independent security certification); packaged WHITE checks PASS; 22/22 Windows C# process/report/quarantine tests PASS; Sanitizer tests PASS; native WPF publish PASS; Inno Setup package PASS; install/WPF UI Automation/second-launch handoff/normal close/repair/uninstall and synthetic user-vault retention PASS; source/submodule/compiler provenance manifest PASS. **Unsigned CI test installer SHA-256: `826DD311F59CAD0E4BEB7A2F1353921CD5C790D90577D73B5C99C40FF4C830AA`**. Artifact upload intentionally skipped on PR, no public signed release. Independent real-device QA, historical DPAPI recovery, Chrome clean-profile, publisher signing and final owner approval remain **NOT RUN/BLOCKED**.

Follow the [one-page owner finish line](docs/OWNER_FINISH_LINE.md) for the exact remaining non-code steps. The repo's root distribution license, trusted publisher signature and final release decision require owner authorization; do not bypass these gates.

## Modules

1. **Repository Security** — the independent Go scanner from OTS GitHub Malware Scanner v2.5. The local server is started on an ephemeral loopback port by the WPF host, is opened inside native WebView2, and is killed when the suite window closes. URL scanning retrieves public GitHub/Bitbucket snapshots; local-folder scanning requires no ScanRepo.dev backend. The Go engine has 95 heuristic rules and format-aware baseline inspection.
2. **Code and Secrets Audit** — a **new, self-contained OTS W.H.I.T.E. preview replacement**, not the deleted standalone codebase. It supports metadata-only heuristic secret checks, selected SAST patterns, optional locally reachable Git-history blob review, explicit incomplete states, and redacted JSON. **Offline dependency CVE/SCA is not implemented in this preview**; its UI control is disabled and CLI requests fail incomplete, never clean. The old engine's exact detection parity is **not established**.
3. **Windows Security** — OTS System Sanitizer's existing .NET 10 core provides quick/deep local inspection, Defender invocation, investigation, encrypted user-scoped quarantine, restore, and reviewed destruction. The original Sanitizer GUI is also bundled for all its existing specialist operations while the consolidated interface is expanded.
4. **Space Recovery** — Sanitizer's cleanup service, with per-file selection and explicit deletion confirmation.
5. **Chrome Extension** — uses the installed `ots-security://` custom URI handler to open the native suite, rather than depending on a fixed local port or ScanRepo.dev.

## Architecture

```text
OTS Security Suite native WPF / WebView2 host (OTSSecuritySuite.exe)
 ├─ child Go process / ephemeral 127.0.0.1 (OTSScannerEngine.exe)
 ├─ child Python audit CLI (OTSWhiteAudit.exe)
 └─ in-process System Sanitizer .NET core
       ├─ Windows threat scan / Defender
       ├─ encrypted quarantine (original DPAPI format)
       └─ cleanup & safe restore
          + optional original OTSSystemSanitizer.exe specialist window
```

The native Windows icon and title-bar icon are both set to the user's supplied shield-bug `.ico`, and the header uses the supplied transparent Ohana Tech Solutions logo. Windows may cache taskbar pins from old releases; unpin the old Chrome-hosted scanner if it remains.

### Building on Windows

You need .NET 10 SDK, Go 1.27.4+ (security-maintained toolchain), Python 3.12, PyInstaller, Inno Setup 6, Microsoft WebView2 runtime, and the **vendored System Sanitizer** source. W.H.I.T.E. source is now included directly in this repository.

```powershell
git clone https://github.com/ohanatechsolutions/OTS_Security_Suite.git
cd OTS_Security_Suite
# GitHub Actions -> Windows build and integration checks -> Run workflow
```

The workflow compiles the native WPF shell, Go engine, PyInstaller CLI, and Sanitizer GUI; it then packs a per-user `OTS-Security-Suite-Setup-x64.exe` with SHA-256 checksums. The installer is **unsigned** until an OTS code-signing identity is configured. Microsoft SmartScreen reputation warnings can occur on unsigned or unfamiliar builds.

For local tests that don't need the Windows toolchain:

```bash
(cd scanner && go test ./... && go vet ./...)
python -m unittest discover -s tools -v
node --check extension/popup.js
```

### Privacy and safety boundaries

All local folder/content scans are read-only unless you explicitly request quarantine or cleanup. The repository scanner reads source, hashes files and analyzes archive entries without executing third-party repository code. The suite does not run untrusted Git hooks or package installation scripts. The local scanner session URL carries a secret and must not be written into logs. Remote repository scans still need a network connection to GitHub/Bitbucket. The replacement W.H.I.T.E. engine has no offline dependency CVE scanner yet. Selecting that CLI check returns an explicitly incomplete report; the UI option is disabled.

The Windows quarantine system is user-scoped and cannot offer kernel-level malware isolation; do not disable Microsoft Defender or other existing protections. Files reported as suspicious are not automatically proven infected. Destructive actions require explicit confirmation.

### Source provenance

- `scanner/`: OTS's independent Go malware scanner source (formerly v2.5)
- `white_engine/ots_white`: self-contained replacement audit engine; the original W.H.I.T.E. source repository was deleted by its owner
- `modules/OTS_System_Sanitizer`: complete vendored System Sanitizer source snapshot (original commit `823ad825`, tree `c56ef135`); no external Git submodule
- `src/OTSSecuritySuite.Desktop/`: new native suite shell
- `extension/`: locally launched repository scanner extension
- `installer/`: Inno Setup packaging

See [BUILD_BLUEPRINT](docs/BUILD_BLUEPRINT.md), [PROJECT_STATUS](docs/PROJECT_STATUS.md), [TEST_EVIDENCE](docs/TEST_EVIDENCE.md) and [RELEASE-GATES](docs/RELEASE-GATES.md). An automated Windows CI installer launch/close and uninstall smoke **passed** for the draft branch; independent Win10/Win11, Chrome, quarantine/restore and malware corpus acceptance has **not** passed. No final publisher-signed release is available.

### Draft branch build evidence — 2026-11-11 HST

- Code commit `59243e6b15c6`, [GitHub Actions run 43172812782](https://github.com/ohanatechsolutions/OTS_Security_Suite/actions/runs/43172812782): **success** for Go tests/vet, 95 rule/12 category assertions, WHITE executable synthetic success/incomplete tests, 8/8 Windows process cancellation/timeout tests, Sanitizer smoke, WPF desktop build and launch/close, installer PE/hash/URI/uninstall.
- Generated Inno Setup SHA-256: `389DBCFE20AE5CB29EAA4F17B5360DEF0DD14841FA4DE7F767301289AFABF092` for `OTS-Security-Suite-Setup-x64.exe`. **Not uploaded/published from the PR**.
- Chrome extension uses `activeTab` only and has no permanent host permissions. Manifest/JS checks passed in CI; clean Chrome profile testing remains.
- UI startup default is 1300×800 DIPs clamped to work area, but human DPI/taskbar verification still required.
- Go scanner count refers to 95 regex heuristic rules across eight categories plus four built-in detection categories; the count is not a guarantee of malware detection quality.
- **Status: integration beta, NOT approved for public release.** Existing standalone tools and encrypted quarantine remain preserved.

### Historical engineering candidate

Source `a2f39b812e61116ecaa3f4813909b662d4149224` adds true RepoScan cancel propagation; local API origin/connection/resource-limit tests; current-and-legacy encrypted quarantine registry, precise destruction-review authorization; JSON report contract checks; stable Windows taskbar AppUserModelID; PNG transparency pixel check; installed extension and license notices; WPF UI Automation; and patched Go 1.27.4. [Current Windows CI](https://github.com/ohanatechsolutions/OTS_Security_Suite/actions/runs/43189027296) **PASSED** at docs head `de277e5fa3bb2d7e1bd8b2aac362a3d9e74ff916` with 15/15 audit/vault tests, real WPF synthetic UI, logo alpha verification, Go security tests, installer/URI/uninstall and preserved vault data. Installer SHA-256 `8872BB5E2039C7B5F6AA13EA7AC52276CB22C80AA88B00FCDEE6271B5BC1C929` (CI-only, unsigned). No installer artifact from this draft PR is uploaded or released; manual Win10/11, Chrome and signing gates remain open.
