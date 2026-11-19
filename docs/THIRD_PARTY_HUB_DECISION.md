# THIRD-PARTY-HUB — Scope Correction / Architectural Decision

**Date:** November 18, 2026 (Hawaii). **Status:** Accepted product direction; implementation under Windows QA.

## Requirement and source decision

- **HUB-001:** The product is a Windows **hub for actual third-party cybersecurity tools**, not an OTS-built integrated scanner.
- **HUB-002:** Remove RepoScan, W.H.I.T.E., Sanitizer, Firewall79X and previous custom network-defense code from the **shipped** application. Preserve source history, separate repo records and quarantined user data; deleting backups or historical repositories is not authorized.
- **HUB-003:** One per-tool navigation surface. Preserve native vendor GUIs by **launching the original installed application**. CLI-only products have no native GUI to embed; expose their genuine local --help safely, not a fake OTS frontend. A real executable icon is shown only when it exists on disk; do not misrepresent generic artwork as a vendor logo.
- **HUB-004:** Strictly **zero required paid subscriptions/APIs**. Do not auto-download, silently execute scanners, elevate privileges, install drivers, start Docker, or launch Strix assessments. All vendor programs remain optional, separately installed, with explicit operator control.
- **HUB-005:** Test an upgrade from the old 0.9.x installer: purge known stale **installation** assets under {app} while preserving original user data and history.

## Approved roster and link provenance

| Program | Function | Upstream | Local hub behavior |
|---|---|---|---|
| Microsoft Defender | Real-time AV | https://support.microsoft.com/windows/stay-protected-with-windows-security | Open genuine Windows Security app |
| ClamAV | On-demand AV | https://www.clamav.net/downloads | Local CLI --help |
| YARA-X | Pattern analysis | https://github.com/VirusTotal/yara-x | Local CLI --help |
| osquery | Endpoint queries | https://osquery.io/ | Local CLI --help |
| KopiaUI | Backup and restore | https://kopia.io/docs/installation/ | Genuine vendor GUI |
| Hayabusa | Windows event hunting | https://github.com/Yamato-Security/hayabusa | Local CLI --help |
| HardeningKitty | Security baseline | https://github.com/scipag/HardeningKitty | Docs only, no elevated PowerShell |
| Microsoft Sysmon | Host telemetry | https://learn.microsoft.com/sysinternals/downloads/sysmon | Docs only, no driver/service installation |
| PE-sieve | Memory inspection | https://github.com/hasherezade/pe-sieve | Local CLI --help |
| TruffleHog | Secret scanning | https://github.com/trufflesecurity/trufflehog | Local CLI --help |
| OSV-Scanner | Dependency vulnerabilities | https://github.com/google/osv-scanner | Local CLI --help |
| Semgrep CE | SAST | https://semgrep.dev/products/community-edition/ | Local CLI --help; verify license/rule terms |
| NVIDIA SkillSpector | AI skill security | https://github.com/NVIDIA/SkillSpector | Docs only pending Windows/provider validation |
| Strix | Opt-in penetration testing | https://github.com/usestrix/strix | Docs only; no autonomous agent runtime shipped |

**Deferred/not installed:** LOKI-RS (the later selection excluded it), separate OpenShell/CC Safety Net, general cloud tools and paid/freemium SaaS vendors. Reassess on an independent license, security, and overlap review. Do not infer that a project being open-source guarantees an unchanged free distribution arrangement in the future.

## Why no vendor code is copied into the installer

Bundling and presenting an upstream executable as a tab would not preserve its true GUI automatically and can violate its distribution or update model. The initial hub keeps tools separate and displays original UI for real GUI apps. True in-window embedding would require vendor-supported interfaces, license review and actual UI acceptance; not claimed. **This is a functional launcher and organizer, not a complete third-party engine integration.**

## Build boundary

- Only `src/OTSSecuritySuite.Hub/OTSSecuritySuite.Hub.csproj` is published by `.github/workflows/windows-build.yml`.
- Legacy scanner, Python audit, sanitizer, Chrome extension and former WPF host source remain in repository **unreferenced by this release job**.
- Inno upgrade removes only `{app}\engines`, `{app}\extension` and explicitly named old application executables, plus the obsolete registry protocol. It must not delete `%LOCALAPPDATA%\OTSSystemSanitizer\Quarantine`, old DPAPI keys or any historical source.
- Native installed tool path settings are per-user at `%LOCALAPPDATA%\Ohana Tech Solutions\Security Suite\hub-tool-paths.json`, stored atomically and revalidated before execution.
- No actual security scan or mutation is performed by the hub; no paid API/network tool installed. Opening an official HTTPS project link is an explicit user action.

## Acceptance and N/A

**Engineering acceptance:** Real `dotnet publish`, Inno PE/install/uninstall, and a successful non-fork PR Windows Actions run with a downloadable unsigned QA artifact. Test instructions and actual SHA-256 must correspond to that exact commit.

**Owner acceptance still necessary:** compare upgraded 0.9.x to hub, tool-specific install/link/native GUI, work-area DPI, path validation failure, CLI stdout caps/timeouts, Windows Security and AV coexistence, no old app tab, accessibility, installer repair/rollback and code signing.

**N/A:** Cloud deployment, payments, multi-tenancy, mobile app stores (this is a per-user Windows launcher). **Not N/A:** Windows installer QA, third-party rights, memory/data protection and execution-boundary safety.
