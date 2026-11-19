# OTS Security Suite Documentation Index

## Current authoritative build direction (read these first)

1. **[TOOLING_DECISIONS.md](TOOLING_DECISIONS.md)** — current selected 14-tool external roster, exclusions, status, free/no-required-subscription constraint, DevSecOps cross-repo tooling and requirement traceability.
2. **[IMPLEMENTATION_ROADMAP.md](IMPLEMENTATION_ROADMAP.md)** — M0–M8 migration order, actual remaining work and per-stage test/release gates.
3. **[PROJECT_STATUS.md](PROJECT_STATUS.md)** — dated evidence and blockers. Read newest entries first; older entries are historical snapshots.
4. **[RELEASE-GATES.md](RELEASE-GATES.md)** and **[TEST_EVIDENCE.md](TEST_EVIDENCE.md)** — verified scoped checks, Windows/Strix security NO-GO items.

## Current or conditional implementation documentation

- [STRIX_INTEGRATION.md](STRIX_INTEGRATION.md) — implemented code-level adapter and specific unresolved actual Docker/agent runtime risks; scope may be redesigned during the external-tools-only migration.
- [DEPENDENCY_INVENTORY.md](DEPENDENCY_INVENTORY.md) — **currently shipped** components vs planned upstream tools; source manifest is not proof of inclusion.
- [AV_001_ANTIVIRUS_COVERAGE.md](AV_001_ANTIVIRUS_COVERAGE.md) — existing read-only Defender/AV status integration.
- [OWNER_FINISH_LINE.md](OWNER_FINISH_LINE.md), [QA_ACCEPTANCE.md](QA_ACCEPTANCE.md), [OPERATOR_GUIDE.md](OPERATOR_GUIDE.md), [RELEASE_RUNBOOK.md](RELEASE_RUNBOOK.md) — historical pre-release and QA procedures; re-baseline before future external-engine launch.

## Historical / superseded reference, not a live scope specification

- [BUILD_BLUEPRINT.md](BUILD_BLUEPRINT.md), [INTEGRATION.md](INTEGRATION.md), [TEST_PRE_RELEASE.md](TEST_PRE_RELEASE.md) describe past native RepoScan-centered builds. Superseded by the tooling register/roadmap.
- [REPOSITORY_CONSOLIDATION_2026-11-18.md](REPOSITORY_CONSOLIDATION_2026-11-18.md), [SANITIZER_REPO_RETIREMENT_2026-11-18.md](SANITIZER_REPO_RETIREMENT_2026-11-18.md), [REPOSCAN_RETIREMENT_2026-11-18.md](REPOSCAN_RETIREMENT_2026-11-18.md) are source/retirement evidence, not authorization to delete repository history.
- [FIREWALL79X_CONSOLIDATION.md](FIREWALL79X_CONSOLIDATION.md), [FIREWALL79X_RETIREMENT_DECISION_2026-11-18.md](FIREWALL79X_RETIREMENT_DECISION_2026-11-18.md), [WHITE_AUDIT_COVERAGE_FIX.md](WHITE_AUDIT_COVERAGE_FIX.md) document retired or earlier OTS implementations.
- Existing GitHub test prereleases are **historical artifacts**, not proof of external-tool product readiness.

**Rule:** A document or README saying a tool exists does not make it integrated. Record code, source commit, tests, license checks, user acceptance and release state separately. Do not silently erase prior evidence.
