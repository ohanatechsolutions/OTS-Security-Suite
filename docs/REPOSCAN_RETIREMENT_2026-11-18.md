# REQ-REPOSCAN-001 — RepoScan replaces W.H.I.T.E.

**Decision:** User approved 2026-11-18. **State:** Engineering candidate, pending CI verification.

RepoScan (`scanner/`, `OTSScannerEngine.exe`) is the suite's only shipped repository security scanner and WPF repository tab. The separate W.H.I.T.E. tab, in-app auditor process and bundled `OTSWhiteAudit.exe` are removed. The existing authorized ephemeral loopback WebView2 scanner, System Sanitizer, firewall tools, Chrome integration, quarantine formats and original branches remain.

## Temporary CI-only security dependency

The Python legacy source (`white_engine/`) and its synthetic tests remain internal to the GitHub Actions runner, which creates a transient `dist/OTSWhiteAudit.exe` **for CI only**. This preserves existing metadata-only secrets/history/source security assertions; it is **not shipped**. Removing the gate without tested replacement could weaken security checks. Old release tags/assets remain historical.

## Unverified parity

RepoScan's 95 heuristic rules and 12 risk categories do **not** imply parity with dedicated secret validation, language-aware SAST, reachable Git history audits or offline dependency CVE/SCA. Build a benign/adversarial synthetic corpus, measure false positives, verify incomplete-scanning reporting and add targeted tests before retiring the CI legacy audit. No zero-finding scan is a guarantee of safety.

## Acceptance gates

1. Source contract validates RepoScan tab present, W.H.I.T.E. UI and shipped executable absent.
2. Windows CI: Go tests/vet, existing legacy CI-only security audit, System Sanitizer and antivirus checks, native WPF compile, Inno installer, smoke installation/UI, uninstaller/quarantine preservation and checksums.
3. Real-device Windows 10/11, DPI, Chrome profile, scan coverage and legacy quarantine QA remain required.

No upstream repo deletion, branch deletion, release publication or paid service is authorized in this change. N/A: cloud/SaaS deployment and database migration; Windows local application.
