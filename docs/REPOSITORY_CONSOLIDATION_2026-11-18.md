# OTS Security Suite — Source-repository consolidation audit

**Date:** 2026-11-18 (HST)  
**Task / requirement:** CONSOL-001 / REQ-CONSOL-001  
**Status:** Source consolidation MERGED, Windows CI PASS. Independent real-machine QA, archive and repository deletion NOT VERIFIED / NOT AUTHORIZED.  
**Merged:** [PR #5](https://github.com/ohanatechsolutions/OTS_Security_Suite/pull/5) → `main@d190b03726c08cc0dfb90d62592ff0355a0f930a`; review branch `work/vendor-sanitizer-for-repo-retirement-20261118`

## Source inventory and decisions

| Repository | Verified source state | Integration | Delete clearance |
| --- | --- | --- | --- |
| `ohanatechsolutions/OTS_System_Santizer` | `main@823ad825dc1358009d3404f54604465c86747eba`; one branch, zero open PRs, one **DRAFT SECURITY-HOLD** installer release | Exact 42-file source snapshot incorporated at `modules/OTS_System_Sanitizer/`, source tree `c56ef135da8b509c88d4175feba1e6055fd4b6f7` | **BLOCKED**: source merge/CI PASS; independent real-device QA and off-repo history/release archive not yet verified |
| `ohanatechsolutions/OTS_WHITE` | 404 / not among accessible account repositories; existing suite docs report owner deleted it | `white_engine/ots_white` is an in-repo **replacement**, not preserved legacy source | **Already inaccessible.** Full old parity cannot be certified without a separately archived source copy |
| `ohanatechsolutions/OTS_Security_Suite` | `main@d00c2ee7ce940195183d77f2e385cadea303bae8` before this change | Native WPF shell, scanner Go source, Python audit replacement, extension and Sanitizer vendoring branch | **Retain** as the only active suite repository |

## Precise migration performed on the work branch

1. Removed the `160000` Git submodule reference at `modules/OTS_System_Sanitizer` and replaced it with the latest upstream's source-tree object, retaining all **42 Git blobs** and executable-bit metadata. GitHub reported the embedded tree **exactly equal** to upstream SHA `c56ef135da8b509c88d4175feba1e6055fd4b6f7`. Source inventory with individual SHA checks is in [SANITIZER_SOURCE_MANIFEST.tsv](SANITIZER_SOURCE_MANIFEST.tsv).
2. Preserved `src/OTSSystemSanitizer.Core/` (including DPAPI/AES quarantine and its original format), original WPF specialist UI, original logo PNG and ICO, smoke tests, MIT license, third-party notices, standalone installer definition and source documentation. The nested old `.github/workflows/` are retained for provenance only and **are not active suite workflows**.
3. Removed root `.gitmodules` and recursive submodule checkout; no repository dependency remains for clean source clones. Original source commit is recorded as `823ad825dc1358009d3404f54604465c86747eba`.
4. Updated `tools/generate-build-manifest.ps1` and the suite Windows workflow to record and verify the **vendored Git tree SHA**, rather than looking for a nested Git repository; added `tools/verify-vendored-sanitizer.ps1` and a workflow gate.
5. Preserved current in-repo `white_engine/` replacement. Did **not** invent old W.H.I.T.E. source, legacy SCA features, credentials, or historical releases.

## Verification / evidence

| Gate | Evidence | Observed state |
| --- | --- | --- |
| Upstream snapshot identity | GitHub `git/trees` API; all 42 `create_blob` hashes compared byte-for-byte to upstream; embedded tree object equals upstream Git tree SHA | **PASS — source identity only** |
| No Git submodule | Review branch tree contains normal source `040000` directory, removes `.gitmodules` and CI recursive checkout | **PASS** on suite `main`: 42 Git blobs, no `.gitmodules`, no gitlinks |
| Windows compilation, tests, real installer | [Windows CI #44301998625](https://github.com/ohanatechsolutions/OTS_Security_Suite/actions/runs/44301998625) at code commit `8a2e27b6`: sanitizer smoke, 22/22 suite audit tests, WPF, Inno installer, installed-app automation and synthetic quarantine retention | **PASS for CI; independent Windows 10/11 QA NOT RUN** |
| Manual actual Windows 10/11, vault restore, icons and clean-install tests | Not performed in this source transfer | **NOT RUN** |
| Original WHITE historical feature parity, offline SCA | Original repo no longer accessible; replacement notice discloses gaps | **BLOCKED / NOT VERIFIED** |
| Git history, issues, tags, DRAFT release archive | Source-file vendoring does not migrate GitHub server-side state | **NOT ARCHIVED** |
| Production signing and new public release | No approval requested or performed | **N/A to source move; required for later public release** |

## Mandatory deletion safety gate

Before deleting `OTS_System_Santizer`, verify **all** of the following:

- The consolidation branch has been merged and the exact 42-file tree is present on **main**. Reconfirm that a clean clone runs without `--recurse-submodules` on your own environment before final deletion.
- The reviewed code commit passed Windows CI, source provenance, Sanitizer smoke, installer creation, and install/uninstall smoke; independently verify the **main-branch** build and a fresh clone as a final pre-deletion check.
- On a disposable Windows machine, verify the standalone Sanitizer specialist tab/application, Quick/Deep inspection, safe synthetic quarantine, restore and cleanup; confirm old encrypted user vaults and standalone users' files remain accessible. Do not test destructive actions against real data.
- Make an **independent offline mirror** of the old repository *including full Git history, refs and tags*; save an archival checksum and restore-test it. Separately inventory and, only after secret review, securely archive the old **unpublished security-hold draft release** and any necessary private records. Do not add the held executable to this public suite repository or distribute it.
- Resolve the reported historical secret-pattern findings in the old repo before broader distribution. Deletion does **not** revoke compromised credentials, remove cached forks, or erase artifacts already accessed.
- Reconcile dependencies, downloads and external links that reference `OTS_System_Santizer`; retain required copyright/license attribution. Confirm no external projects still rely on that source path.
- Record human owner sign-off in the project evidence log, then **the owner** may manually delete the old repository. No AI deletion is authorized by this task.

**Recommendation:** Keep the old repository intact during migration. After the source integration is verified, prefer archiving it before permanent deletion; if deletion remains desired, retain the verified offline Git mirror and release/evidence archive.

## 2026-11-18 retirement readiness update

Sanitizer's upstream `main` and Suite's embedded directory have the same source-tree SHA. The live upstream has one branch, no open PRs and a security-held draft release; archival of its full Git history and draft assets is still **NOT VERIFIED**, so deletion is **BLOCKED**. A fail-closed owner-side archive and restore tool and detailed acceptance ledger are in [SANITIZER_REPO_RETIREMENT_2026-11-18.md](SANITIZER_REPO_RETIREMENT_2026-11-18.md). Do not mistake successful CI or a Git tree match for verified release-asset backup, historical secret review, real Windows QA or owner signoff.
