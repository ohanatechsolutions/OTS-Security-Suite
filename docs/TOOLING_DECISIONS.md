# OTS Security Suite — Current Tool Decisions (Source of Truth)

**Decision date:** November 18, 2026 (Hawaii). **Status:** APPROVED DIRECTION / IMPLEMENTATION IN PROGRESS.  
**Product:** OTS Security Suite, Windows 10/11 x64 security-tool hub by Ohana Tech Solutions.  
**Authority:** Owner's latest direction supersedes older code-first proposals and earlier RepoScan/W.H.I.T.E./Sanitizer consolidation plans. **This file describes intended adoption, not proof of a working integration.**

## Read this before developing

1. **Use reviewed external cybersecurity tools as the security engines.** Do not expand the old OTS-built RepoScan, W.H.I.T.E., Sanitizer, or Firewall79X engines as the future product. The WPF shell may remain as a neutral launcher/orchestrator until a better integration shell is accepted; it must not be marketed as a proprietary replacement detector.
2. **Every adopted tool must work without mandatory subscription, expiring trial, paid API, or cloud service.** Prefer audited, pinned open-source versions and offline/local operation. Existing Windows licensing, storage hardware, electric power, and optional cloud storage are separate costs; never secretly require them.
3. **Preserve the older Git history, release assets, user vaults and source for recovery.** "Remove from product" is **NOT** permission to delete repositories, files, signatures, or legacy quarantine records. Existing source in Git is **historical, not part of the target tool list**.
4. **Do not mislabel a tool integrated because its name appears here.** Status vocabulary: SELECTED = target scope only; WIRED = actual integration/code exists; VERIFIED = linked tests proved the stated behavior; BLOCKED = required test/licensing/security work missing; RETIRED = excluded from target product.
5. **No automatic installs, signature updates, daemon startup, security-setting changes, quarantines, deletion, executable sandbox runs, or releases.** Require operator consent and risk-based isolation. Do not disable Defender to install a second AV.

## Selected external runtime-tool roster

Each row is individually tracked. **SELECTED** is an architecture decision, **not an implementation or Windows compatibility certification**. The target has 14 named tools; some operate in CI or as optional tabs instead of a persistent endpoint service.

| ID | Selected tool and source | Dedicated job / integration boundary | Current truthful status | Acceptance before "integrated" |
|---|---|---|---|---|
| TOOL-01 | [Microsoft Defender / Windows Security](https://learn.microsoft.com/defender-endpoint/microsoft-defender-antivirus-windows) | Existing Windows AV owner; read status and optionally request a native scan only when active | **WIRED — status/AV coexistence tested**; not our own AV engine | Real Defender and third-party AV state/device QA; never disable another AV |
| TOOL-02 | [ClamAV](https://github.com/Cisco-Talos/clamav) | Separate on-demand file/drive detection with signed database/update controls | **SELECTED — NOT WIRED** | Windows CLI/engine and database provenance, output parser, quarantine policy, GPL/notice review, resource safety |
| TOOL-03 | [YARA-X](https://github.com/VirusTotal/yara-x) | User-selected binary/file signature/rule scanning | **SELECTED — NOT WIRED** | Versioned rule-set permissions/license, explicit scan scope and bounded memory/timeout; false positives handled |
| TOOL-04 | [osquery](https://github.com/osquery/osquery) | Read-only endpoint processes, services, persistence and system inventory | **SELECTED — NOT WIRED** | Least-privilege queried data, privacy/retention, bounded scheduled/interactive queries |
| TOOL-05 | [Kopia](https://github.com/kopia/kopia) | Optional user-requested encrypted local backup and restore | **SELECTED — NOT WIRED** | Local-storage mode at $0 software fee, key custody, backup/restore tests, no unapproved cloud charges |
| TOOL-06 | [Hayabusa](https://github.com/Yamato-Security/hayabusa) | Offline Windows Event Log investigation and timelines | **SELECTED — NOT WIRED** | Log availability, rules provenance and **AGPLv3 + separate rule-license** review, honest event attribution |
| TOOL-07 | [HardeningKitty](https://github.com/scipag/HardeningKitty) | Windows hardening **assessment first** | **SELECTED — NOT WIRED** | Baseline/version attribution, read-only first; explicit rollback and admin consent before changes |
| TOOL-08 | [Microsoft Sysmon](https://learn.microsoft.com/sysinternals/downloads/sysmon) | **Optional** Windows event telemetry, never silently installed | **SELECTED OPTIONAL — NOT WIRED** | EULA/redistribution review, install consent, event noise and retention budgets, removal/rollback |
| TOOL-09 | [PE-sieve](https://github.com/hasherezade/pe-sieve) | **Optional** per-process implant/in-memory inspection | **SELECTED OPTIONAL — NOT WIRED** | Explicit process selection, permissions, dump privacy, limits, false positives, binary provenance |
| TOOL-10 | [TruffleHog](https://github.com/trufflesecurity/trufflehog) | Primary repository/commit-history credential scanner | **CI WIRED through shared baseline**; not a desktop tab | Confirm current pinned shared action, scope/secret-redaction and PR checks; runtime GUI integration separate |
| TOOL-11 | [OSV-Scanner](https://github.com/google/osv-scanner) | Dependency manifest and lockfile vulnerability audit | **CI WIRED** via repository's OSV reusable workflow; no desktop adapter | Confirm latest full CI result; fail closed on unavailable database/manifests, record unsupported ecosystems |
| TOOL-12 | [Semgrep CE](https://github.com/semgrep/semgrep) | Static analysis of supported languages | **CI WIRED through shared baseline**; not a desktop tab | Verify exact ruleset, paid-service independence, language coverage, and action failures |
| TOOL-13 | [NVIDIA SkillSpector](https://github.com/NVIDIA/SkillSpector) | Scan agent skills (such as SKILL.md) for prompt injection/exfiltration | **CI CONDITIONAL**, static mode when skills exist; no desktop adapter | Report **N/A — no agent skills** rather than clean when none; no mandatory external model/provider |
| TOOL-14 | [Strix OSS CLI](https://github.com/usestrix/strix) | **Optional**, explicitly authorized autonomous pentest against an isolated test copy using a self-hosted local AI model | **ADAPTER WIRED/SCOPED TESTS PASS**; **REAL STRIX/Docker RUNTIME NOT VERIFIED** | Confirm CLI flags/model, Docker egress containment, cleanup, explicit target authorization, zero paid API, local reports |

**No required license/subscription fee** is the target. This **does not guarantee future pricing or all redistribution rights**. Check each project's current license and asset/rules licenses at the version selected for bundling. In particular ClamAV is GPLv2 for its core with additional third-party terms, YARA-X is BSD-3-Clause, Hayabusa is AGPLv3 with separate detection-rule terms, PE-sieve is BSD-2-Clause, Kopia is Apache-2.0, and Strix OSS CLI is Apache-2.0. These are sources to assess, **not a completed legal distribution signoff**.

## Explicitly excluded or not adopted

| ID | Component | Decision | Why / preservation rule |
|---|---|---|---|
| RET-01 | Original OTS RepoScan / Go heuristic engine | **RETIRED FROM TARGET** | Superseded by reviewed external detection and audit engines. **Still launches in the existing main-branch installer**; migration is P0, not done |
| RET-02 | OTS W.H.I.T.E. and Python replacement auditor | **RETIRED FROM TARGET** | Use TruffleHog + Semgrep CE + OSV-Scanner instead. Old replacement still runs in CI; move to external tools only after security equivalence checks |
| RET-03 | OTS System Sanitizer | **RETIRED FROM SHIPPED PRODUCT** | Already excluded from product UI/package in earlier PR; archived source and historical vaults stay preserved |
| RET-04 | OTS Firewall79X | **RETIRED / NO INTEGRATION** | Do not replace or compete with built-in Windows Firewall or silently install a second firewall |
| RET-05 | LOKI-RS | **NOT SELECTED FOR V1** | Redundant with planned ClamAV/YARA-X/PE-sieve; revisit only if coverage evidence warrants it |
| RET-06 | NVIDIA OpenShell as a mandatory product module | **NOT SELECTED AS A STANDALONE TAB** | Agent sandbox technology remains **under evaluation**; runtime Strix isolation is a BLOCKER, not N/A |
| RET-07 | Hosted Strix cloud, paid AI APIs and cloud-only scanners | **REJECTED** | Incompatible with $0 mandatory subscription/API constraint |

**Note:** Built-in Windows Security, standard Windows Firewall, and a neutral OTS application shell may still be used. "External-tools-only" governs detection/auditing engines, **not the app's own UI or orchestration code**. Do not silently claim the original source is gone.

## Separate universal DevSecOps repository controls

These are **engineering security gates**, not all end-user desktop tabs. In this repository:
- [Universal Security workflow](../.github/workflows/ots-universal-security.yml) calls the [shared OTS_DevOps baseline](https://github.com/ohanatechsolutions/OTS_DevOps/blob/main/.github/workflows/security-baseline.yml) and the OSV reusable workflow.
- Existing shared baseline includes **Trivy filesystem (vulnerabilities/secrets/misconfiguration), TruffleHog, Semgrep CE, language-appropriate tests and lint, conditional NVIDIA SkillSpector**, and applicable .NET/Go checks. Pin and review upstream reusable workflow versions.
- **OSV-Scanner** is configured with failure-on-vulnerability; check actual runs before claiming PASS.
- **Branch protection / required checks** must be reviewed in GitHub settings; YAML cannot prove rulesets are enforced.
- **OWASP ZAP**: apply only to an authorized isolated web test target. **N/A for a pure native UI without a test HTTP service**; evaluate the local RepoScan WebView2 service only if it is retained during migration and isolated appropriately.
- **NVIDIA OpenShell / sandboxing**: runtime autonomous agents (Strix) require a defensible sandbox/egress policy even if OpenShell itself is not adopted. The shared policy currently describes the Suite as "N/A unless autonomous agents"—for the **Strix path this is BLOCKED**, not N/A. Update shared policy before declaring cross-repo control complete.
- **Build and artifact verification**: Windows packaging, MZ/PE validation, SHA-256/provenance and independently installed-app checks; a passing build alone is not release certification.
- All 10 universal protocol topics must state **PASS**, **FAIL**, **BLOCKED**, **NOT RUN**, or **N/A with reason**, never blanket "all tools installed."

## Traceability and next-step contract

| Requirement | Acceptance evidence needed | Present state |
|---|---|---|
| REQ-TOOL-001 | Each SELECTED tool has version, official download/source, cost/licensing gate, safe CLI contract, tab/runner, synthetic positive/negative tests and real Windows evidence | **IN PROGRESS**, only partial existing integration |
| REQ-TOOL-002 | Old RepoScan/WHITE/Sanitizer/Firewall79X engines no longer ship or launch, without deleting legacy data | **BLOCKED** — RepoScan still shipped; Sanitizer removed from product |
| REQ-TOOL-003 | AV coexistence, safe on-demand scans, no automatic destructive action | **IN PROGRESS** — Windows status contract only |
| REQ-TOOL-004 | Autonomous Strix only with owned targets, isolated disposable environment, local model and verified network controls | **BLOCKED** — code-level tests yes, sandbox runtime no |
| REQ-TOOL-005 | Zero mandatory paid software, APIs or cloud services; per-component license and rules review | **IN PROGRESS** — licensed candidate roster, distro reviews incomplete |
| REQ-TOOL-006 | All-repo DevSecOps gates plus explicit per-control N/A, required checks and release provenance | **IN PROGRESS** — workflows installed, settings/runtime gates unverified |
| REQ-TOOL-007 | One accurate current doc inventory, roadmap, release gate and evidence log | **IN PROGRESS** — docs change pending merge and review |

**Current and historical evidence:** [PROJECT_STATUS](PROJECT_STATUS.md), [TEST_EVIDENCE](TEST_EVIDENCE.md), [RELEASE-GATES](RELEASE-GATES.md), [STRIX_INTEGRATION](STRIX_INTEGRATION.md). See [IMPLEMENTATION_ROADMAP](IMPLEMENTATION_ROADMAP.md) for implementation order and explicit tests.

**Applicability / N/A:** No paid APIs, external tenant deployment, or automatic network pentest (excluded by owner). Dynamic ZAP HTTP scanning N/A without an isolated test endpoint. SkillSpector N/A where no agent skills exist. Full Strix sandbox and owner acceptance **NOT N/A**.
