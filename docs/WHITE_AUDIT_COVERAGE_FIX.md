# W.H.I.T.E. audit-coverage hardening — 2026-11-15

**Task:** BUG-WHITE-001 / REQ-005, REQ-013, TH-007, TEST-WHITE-009..012  
**Branch:** `work/white-incomplete-audit-hardening-20261115` based on `preview/ots-security-suite-v0.9.0-test1@db1b9ca1c29d8cd15fc23344f955d50c4e9fbbc8`  
**State:** IN PROGRESS pending fresh cross-platform CI and manual Windows acceptance; **release NO-GO**.

## Observed issue and user outcome

Python's `os.walk` defaults to `onerror=None`, silently ignoring directory enumeration errors. An attempted full-source audit could consequently report `complete: true` and an apparently clean result despite unreadable subdirectories. Both the worktree and Git-history traversal also applied their object-count budget only to successfully scanned text, so large volumes of skipped binary or oversized content could evade that budget.

The intended behavior is: every directory enumeration failure marks source checks failed and the overall JSON incomplete (exit code 2); reports never expose filesystem exception paths; and the configured inventory budgets count *all in-scope traversed file/object entries*, not only successfully scanned text.

## Implementation and traceability

| Requirement / threat | Change | Regression |
|---|---|---|
| REQ-005 / TH-007 | `os.walk(..., onerror=...)` tracks unreadable directories and fails source checks closed | `test_unreadable_subdirectory_fails_closed_without_leaking_path` |
| REQ-013 | Generic directory failure message never includes exception path | same test |
| REQ-005 / TH-007 | Worktree inventory counts in-scope skipped entries toward `MAX_FILES` | `test_worktree_file_budget_counts_uninspected_files` |
| REQ-005 / TH-007 | Explicit user-approved excludes remain outside inventory budget | `test_explicit_exclusions_do_not_consume_budget` |
| REQ-005 / TH-007 | Historical traversal counts all enumerated in-scope objects toward `MAX_HISTORY_BLOBS` | `test_history_object_budget_counts_uninspected_objects` |
| REQ-005 / UI | Zero-finding status explicitly refers only to inspected supported files | source review; real GUI wording acceptance still required |

**Compatibility:** Existing JSON v1.0 keys and exit code 0/1/2 contract are preserved. Added counters `inventoried_files`, `unreadable_dirs` and `inventoried_history_objects` are additive. Excluded paths and intentionally unsupported binaries can still be skipped, so `complete` must not be interpreted as comprehensive malware detection.

## Verification gates

- Python regression and full offline WHITE suite — **NOT RUN** until build CI reports a result.
- Windows WPF host compile, UI integration, Inno installer and all existing regressions — **NOT RUN** on this change until CI.
- Real Windows 10/11 x64, 100/150/200% DPI, Chrome clean profile, genuine legacy encrypted vault, independent scanner efficacy/security review, official publisher signing and human GO approval — **NOT RUN / BLOCKED** by release-owner acceptance.
- **N/A:** Web SaaS, cloud database, payments, AI agent and mobile-app testing — these components do not exist in the Windows suite.

Do not reuse the earlier `v0.9.0-test.2` installer hash as proof for this branch. A new artifact must be separately built, installed, hashed, and approved before any distribution.
