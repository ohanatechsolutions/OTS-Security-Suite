# CONSOL-002 — Standalone repository retirement / owner handoff

**Status: BLOCKED for irreversible deletion (2026-11-18).** This page is an actionable gate, not permission to delete.

## Confirmed from connected GitHub, 2026-11-18

| Repository | Verified state | Decision |
| --- | --- | --- |
| `ohanatechsolutions/OTS_WHITE` | GitHub 404 and absent from owner repository inventory | Already inaccessible; do not create or delete a substitute. Full legacy parity cannot be proven from the vanished source. |
| `ohanatechsolutions/OTS_System_Santizer` | Public; `main@823ad825dc1358009d3404f54604465c86747eba`; exactly 1 branch; 0 open PRs; 1 unpublished DRAFT held release; historical repository size reported ~538,895 KiB | **Do not delete until this page's gates pass** |
| `ohanatechsolutions/OTS_Security_Suite` | `main@4edc328eb268ebfa569260b1bbb5ea76567b38d7`; Windows CI #44415251281 success; vendored Sanitizer tree SHA `c56ef135da8b509c88d4175feba1e6055fd4b6f7` | Retain as active repository |

The Sanitizer upstream commit's Git tree is **exactly** `c56ef135da8b509c88d4175feba1e6055fd4b6f7`, matching the **actual directory Git SHA** of `modules/OTS_System_Sanitizer` in suite `main`. That is an exact 42-file snapshot, not a copy of the original independent Git history or GitHub release records. The Windows CI checks the original UI source as part of bundled Sanitizer and tests the suite installer; this does not independently prove genuine historical DPAPI vault recovery.

### Security-hold release (do not distribute)

GitHub release ID `446563929`, tag text `v0.4.0-legacy-security-hold`, **draft**, with exactly two assets:
- `OTSSystemSanitizerSetup-x64.exe` — 46,476,112 bytes; SHA-256 `6aee66341f0cb5870be9832e04029f9fc5d524e63dc387d7c2bbc323c90bb358`
- `OTSSystemSanitizerSetup-x64.sha256` — 98 bytes; GitHub asset SHA-256 `37ab19364d3c5a02c878f71968bf83c6162235a19c9670f21b8356e0a7bc9ddd`

The old repository's `docs/INSTALLER_DISTRIBUTION_AUDIT.md` documents a **failed publication-security scan** for historical potential secrets. The binary must remain restricted and not be copied into the public Suite or run as part of the backup.

## Owner's offline retirement preparation (Windows PowerShell)

1. Install Git for Windows and GitHub CLI; run `gh auth login` **locally**, without sharing tokens, passwords, or the resulting archive with ChatGPT. Check free space; the source repository history is >500 MiB and the mirror + portable bundle + restored mirror can require multiple gigabytes.
2. Choose a private encrypted local volume with a **new, nonexistent** destination directory, e.g.:

   ```powershell
   pwsh -NoProfile -File .\tools\backup-retiring-sanitizer.ps1 -Destination "D:\PrivateBackups\OTS-Sanitizer-Retirement-2026-11-18"
   ```

3. The tool fails closed on unexpected upstream commit, source-tree mismatch, broken Git object history, failed bundle restore, missing/changed draft release assets or checksum mismatch. It writes a local JSON manifest and GitHub issue/PR/release/run metadata. Nothing is uploaded or deleted.
4. **Independently** copy the resulting complete archive to a second encrypted/offline device. On that device run a new restore check and verify the `OTS_System_Santizer-all-refs.bundle` SHA against the original manifest. The local tool **does not** mark offsite replication verified.
5. Run genuine legacy Windows 10 and Windows 11 x64 acceptance on disposable test machines: Sanitizer GUI, Quick/Deep scans with harmless fixture files, original user-context DPAPI quarantine restoration using backed-up historical vault data, cleanup safety, install/uninstall and 100%/150%/200% DPI. Record the actual results in the suite's `docs/QA_ACCEPTANCE.md`. Do not run the security-held historical installer. Use a freshly CI-built Suite preview for these acceptance checks, not an old tagged prerelease.
6. Privately review old historical potential secret findings and **rotate/revoke any real exposed credentials**. A deleted repo cannot revoke secrets or erase copies of leaked data. Review downstream clone, issue, README, release, wiki, workflow-artifact and external links. The tool preserves issue/PR metadata, but **does not yet archive Actions logs/artifacts or GitHub wiki**; preserve any required such records separately before deletion.
7. Record owner approval and evidence references. Only then may the owner manually delete the old repository. Prefer GitHub **Archive** (reversible) over Delete (destructive) if you primarily want to reduce repository clutter.

## Evidence ledger (do not check before actual verification)

| Gate | State now | Evidence / next required proof |
| --- | --- | --- |
| Exact source merged | **PASS** | GitHub live tree, upstream commit/tree, merged PR #5 |
| Windows GitHub CI of consolidated suite | **PASS** | #44415251281 |
| RepoScan replaces shipped W.H.I.T.E. UI | **PASS for code/CI** | merged PR #11; historical W.H.I.T.E. gate remains internal CI only |
| Full historical Git mirror / bundle / restore | **BLOCKED** | Must run private backup tool on owner's Windows machine and retain manifest |
| Draft-security-hold release copied + asset digest checked | **BLOCKED** | Private owner credentials and local encrypted archive required |
| Independent second offline copy | **BLOCKED** | Offsite restore/checksum evidence required |
| Historical secret review and credential rotation | **BLOCKED** | Security owner review, no live secrets in public issue |
| Genuine historical DPAPI vault restore and Win10/11 UI acceptance | **BLOCKED** | Owner test evidence with authorized original user context |
| Owner approval and irreversible deletion | **BLOCKED** | Explicit dated owner signoff following above evidence |

**N/A with justification:** Cloud deployment / database migration (local Windows application). New license acquisition / code-signing is not required to preserve source; it remains a release gate if future distribution is approved. Original W.H.I.T.E. repository deletion action is N/A because the repo is currently inaccessible (404).

**Do not commit the private archive, its assets, historical binaries, user vaults, or the generated local manifest into the public Suite.**
