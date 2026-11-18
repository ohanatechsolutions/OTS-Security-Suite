# Firewall79X — final repository retirement decision (2026-11-18)

**Decision:** Product sunset READY; GitHub **Archive** is the recommended reversible retirement action; GitHub **Delete** is NO-GO.

**Original repository:** https://github.com/ohanatechsolutions/OTS_Firewall79X
**Inspected original main:** `1ac9a1e86c2b13f5968618118e9fdfa8fb883762`.
**Active successor:** https://github.com/ohanatechsolutions/OTS_Security_Suite
**Suite CI on inspected main:** https://github.com/ohanatechsolutions/OTS_Security_Suite/actions/runs/44415251281 — SUCCESS at `4edc328eb268ebfa569260b1bbb5ea76567b38d7`.
**Current direct user-request:** retire the original repo without losing capabilities or creating vulnerabilities.

## Verified completed (evidence)

- [x] Original standalone product README and `STANDALONE_SUNSET.md` point new consumers and future work at Security Suite; old repo PR #49 merged.
- [x] Old repo has no open PRs as inspected on 2026-11-18.
- [x] Security Suite PR #8 migrated a **read-only** Windows Firewall profile and TCP connection dashboard, then passed CI.
- [x] Security Suite PR #9 migrated conservative firewall-rule overlap hints, tests, MIT license and installer attribution, then passed CI.
- [x] Security Suite PR #10 added read-only supported Windows Firewall and Defender activity observations with source attribution, tests, then passed CI.
- [x] Current Source Git file inventory saved as `FIREWALL79X_SOURCE_MANIFEST_CURRENT.tsv` (159 tracked files from the stated source commit). **Not a full copy of Git objects.**
- [x] Release metadata and asset inventory saved as `FIREWALL79X_RELEASE_ASSETS_2026-11-18.json` (15 public releases, 193 downloadable assets, 3,558,587,784 bytes / ~3.56 decimal GB). **Contains URLs/digests/metadata, not asset bytes.**
- [x] Remaining functional and archival decision work moved to Security Suite issue #12; see `FIREWALL79X_CONSOLIDATION.md`.

## Two different possible outcomes

### A — Archive the GitHub repository (RECOMMENDED; ready for owner action)

A GitHub repository archive becomes read-only, indicates it is no longer actively maintained, and can be unarchived if source/reference is needed. Existing commits, release assets, issues and PRs stay associated with the original repository rather than relying on incomplete out-of-repo backups. The original project has already been labeled superseded.

**Owner UI action:** GitHub `ohanatechsolutions/OTS_Firewall79X` → Settings → Danger Zone → **Archive this repository** → follow the confirmation dialog.

**This is deliberately an OWNER action.** Current connected GitHub operations do not expose an authorized repository-settings/archive operation. **Archiving has NOT been performed by the assistant.**

Archive is suitable for **ceasing separate development and retaining all the historical record**. It does not certify previously incomplete features, guarantee indefinite hosting or resolve historical secret exposure. Before archiving, ensure no external deployment depends on this repository's Actions; it has no open PRs.

### B — Permanently DELETE the GitHub repository (NOT CLEARED)

| Gate | Current evidence | Verdict |
|---|---|---|
| All refs/tags/history and LFS included in a separately held backup | File names and blob IDs only; full git mirror not created | BLOCKED |
| Real independent restore test of that Git backup | NOT RUN | BLOCKED |
| All 193 published asset bytes independently retained and checksummed | Manifest only; ~3.56 GB remains hosted in old GitHub Releases | BLOCKED |
| All release descriptions, issues, PR review history and metadata recovered elsewhere | Not archived as a verified restorable dataset | BLOCKED |
| Unique existing Firewall79X behaviors migrated or **explicitly declined by owner** | Suite read-only slices only; privileged service/extra detections, logging, canaries/recovery not fully migrated; the old 79 specs were not all certified | BLOCKED |
| Historical secrets/Gitleaks findings reviewed and actual credentials rotated if needed | Earlier release scan failures on old repository; no validated disposition | BLOCKED |
| Real Win10/11 and no-admin coexistence/upgrade/rollback acceptance of Suite | Windows Actions CI pass only; independent device QA NOT RUN | BLOCKED |
| Owner explicit final approval **after** these checks | Not yet obtained | BLOCKED |

**Do not confuse** a source ZIP, source manifest, release manifest, current `main` build, or a successful PR with the full independent backup and end-user acceptance required to permanently delete a repository. Deleting a repository can break historic URLs and erase Actions/release records; it is not a substitute for rotating any exposed secret.

## Engineering decisions

- Retain Defender Antivirus and Windows Firewall as the base protections; Suite read-only network monitoring does not disable them.
- Avoid importing old Firewall79X's privileged Windows service, packet-level filtering, machine hardening, extra firewall driver or automatic rule blocking without independent security/Windows compatibility evidence. This is **intentional exclusion pending owner signoff**, not a completed migration.
- Retained MIT analyzer code has in-package attribution; external engine relicensing requires a separate review.
- Mark remote Linux/mobile/app store tracks **N/A — Windows desktop-only product**, per 05 platform-scope policy.
- Original repo deletion is an **irreversible operation** and remains unperformed. Archiving is the recommended way to accomplish the user's product-consolidation goal without losing source or published binaries.

**Handoff:** The user may safely stop using the standalone product and perform **GitHub Archive** to remove it from active maintenance. The user should NOT yet press **Delete this repository**.
