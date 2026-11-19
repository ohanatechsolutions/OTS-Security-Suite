> **2026-11-18 HST — M1 engineering update:** Built and Windows CI-tested a separate OTS Security Suite native WPF hub-only application that does not publish the old RepoScan/WHITE/Sanitizer/Firewall79X engines. [Windows build and install QA](https://github.com/ohanatechsolutions/OTS-Security-Suite/actions/runs/44470904063) and [universal security](https://github.com/ohanatechsolutions/OTS-Security-Suite/actions/runs/44470904447) **PASS** at reviewed code commit `5a5361ada5242cb522eb853a98b15b11550d8355`. M1 remains **PARTIAL** because installation *over a real prior 0.9.x build* and real legacy-vault restore are not yet verified. M2–M7 vendor execution is not completed merely by displaying tool links and CLI help. M8 signed release **NO-GO**.

# OTS Security Suite — External-Tool Migration Roadmap

**Decision baseline:** November 18, 2026. **Owner direction:** stop extending old OTS-coded security engines and use the reviewed free external tools. **Engineering status:** target architecture approved as direction, **migration NOT complete**.

**Start every new engineering session with** [TOOLING_DECISIONS.md](TOOLING_DECISIONS.md), then [PROJECT_STATUS.md](PROJECT_STATUS.md), [RELEASE-GATES.md](RELEASE-GATES.md), current git branch/head and Windows Actions. Historical docs may describe earlier functionality; this roadmap and tool register supersede those passages for **future development**.

## Release-safe migration order

| Stage | Work / deliverable | Definition of done and evidence | State |
|---|---|---|---|
| M0 — inventory and decision lock | Confirm 14 external candidates, exclusions, $0 pricing, safety and versioned license acceptance | Tool register + per-component sourced license/cost log; explicit change-control decisions | **IN PROGRESS** |
| M1 — remove proprietary engine reliance | Separate UI from RepoScan Go child service and old Python W.H.I.T.E. engine; do not package/launch either. Preserve archived source and old vault data; keep app operable throughout | File/tree diff, clean WPF build, negative package assertions, application launch without old engines, synthetic uninstall/upgrade retention checks | **PARTIAL** — CI hub build/launch PASS, real 0.9.x upgrade and vault retention NOT RUN |
| M2 — endpoint inspection | Defender passive status, ClamAV on-demand scanning, YARA-X rule scanning, osquery inventory | Contract tests for process timeout, cancellation, bounded output, failures/unknown states; real isolated Windows 10/11 test | **PARTIAL** — Defender status only |
| M3 — Windows investigation | Hayabusa offline EVTX, PE-sieve opt-in process inspection, Sysmon as separately consented optional telemetry | Correct source attribution, rules licensing, elevation and consent boundaries, privacy and retention checks | **NOT STARTED** |
| M4 — resilience and posture | Kopia opt-in encrypted local backups/restores, HardeningKitty audit-only posture assessment | Successful restore and rollback with fixtures; no silent system changes or paid storage | **NOT STARTED** |
| M5 — repository audit | TruffleHog, Semgrep CE and OSV results in appropriate desktop or CI views, using upstream tools rather than cloned detection logic | Typed result schemas, fail-closed unsupported/errors, version evidence, secret redaction, no paid SaaS | **CI PARTIAL** — runtime UI not done |
| M6 — agent safety | Existing Strix adapter revalidated against official OSS CLI; controlled Docker VM, local model, container lifecycle and outbound network egress | Explicit written scope, negative tests, actual bounded run against owner fixture, no unauthorized egress, no residual containers, reports secured | **BLOCKED** — runtime not verified |
| M7 — specialized safety | Conditional SkillSpector for local skills, OWASP ZAP only against test HTTP service, agent isolation policy and branch protection | Exact applicable/verified N/A per repo, green CI evidence, rule checks and zero unapproved cloud transfers | **IN PROGRESS** — shared CI partly wired |
| M8 — quality, licensing, delivery | Full dependency SBOM, notices/license compatibility, pinning, independent Windows QA, accessibility/DPI, checksum and installer provenance | Approved unsigned owner-test artifact if authorized; production signing/owner go/no-go before public release | **BLOCKED** |

## Implementation rules

1. Preserve source and history; **do not delete** legacy source, releases, branches, vaults, or old repositories as part of a UI migration.
2. Integrate one **real upstream engine** at a time with official CLI/binary provenance; no placeholder scanner, fake counts, or tests that only inspect strings.
3. Every action must report tool name, version, scan scope, completed/incomplete/unsupported status, exit code, findings provenance and duration. A tool process crash must never mean "clean."
4. Gate execution by consent. Defender remains responsible for the existing AV; ClamAV should start as **on-demand only**, not a conflicting real-time service.
5. HardeningKitty remains audit-only until separate explicit authorization and rollback are implemented. Sysmon requires install/uninstall consent; PE-sieve requires scoped process permission.
6. For Kopia, demonstrate restore on disposable files before promising backup protection. Keep encrypted keys local; no mandatory paid cloud target.
7. Strix is an **autonomous agent**, not merely a scanner. Docker network/host safety must be verified independently; numeric-loopback LLM settings alone do not isolate the agent.
8. Run lint/type/unit/integration and Windows package tests, then update project status and CI source-to-commit evidence. **No merge/release claim without the exact passing run and signoff.**
9. Do not delete current old Go/Python build workflows until replacement path runs cleanly. When retired, remove old engine from installer/manifest and stop running legacy CI source audit as the product gate; preserve archival tests separately.

## Program tracking and handoff

- **Project owner:** Ohana Tech Solutions. **Platform:** Windows 10/11 x64.
- **Cost:** $0 mandatory security tools/APIs; no hidden paid cloud/model/service dependency. All redistribution licenses pending individual signoff.
- **Source-of-truth tool register:** [TOOLING_DECISIONS](TOOLING_DECISIONS.md); this document provides implementation sequencing, not a different tool list.
- **Verified older integration:** [Strix/sanitizer CI #44463640720](https://github.com/ohanatechsolutions/OTS-Security-Suite/actions/runs/44463640720) demonstrates old WPF build and Strix synthetic adapter policy only, **not** the new external-tool migration.
- **Existing dev security:** [OTS Universal Security workflow](../.github/workflows/ots-universal-security.yml) covers shared baseline + OSV; confirm actual latest run and branch rules.
- **Release state:** NO-GO until independent testing and license/security approval.

## N/A / deferred decisions

- N/A for an online hosted multi-tenant backend: the application is local Windows-first.
- N/A for subscription APIs, hosted Strix cloud or compulsory external AI inference: violates owner cost requirement.
- N/A for ZAP against unrelated public websites; any web DAST is limited to explicit isolated authorized test scope.
- N/A for SkillSpector when no agent skills are present, with evidence.
- **NOT N/A:** autonomous agent containment, vendor licensing, safe Windows install/uninstall and old-vault data preservation.
