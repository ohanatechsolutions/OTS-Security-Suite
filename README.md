<p align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="art/SecuritySuite-header-dark.png" />
    <source media="(prefers-color-scheme: light)" srcset="art/SecuritySuite-header-light.png" />
    <img src="art/SecuritySuite-header-light.png" width="100%" alt="OTS Security Suite original branded banner" />
  </picture>
</p>

# OTS Security Suite

> **Owner test installer now available:** [**Download v0.10.0-test.1**](https://github.com/ohanatechsolutions/OTS-Security-Suite/releases/download/v0.10.0-test.1/OTS-Security-Suite-Setup-x64.exe) · [GitHub pre-release](https://github.com/ohanatechsolutions/OTS-Security-Suite/releases/tag/v0.10.0-test.1) · [verified Windows build](https://github.com/ohanatechsolutions/OTS-Security-Suite/actions/runs/44504685976). This is an **unsigned test preview**, **not a production-ready antivirus/security suite**. The 14 selected upstream products have navigation and safe original-tool entry points; their complete scanners and runtime integrations are **not all implemented**. Historical `v0.9.0` releases still launch old OTS scanners and are obsolete for the approved product.

<p align="center"><strong>One Windows hub. Proven external security engines. No required subscription.</strong></p>

<p align="center">A modular Windows security-tool workspace by <strong>Ohana Tech Solutions</strong>. The next version is a hub for previously reviewed free cybersecurity tools, rather than another in-house malware scanner.</p>

<p align="center"><img src="https://img.shields.io/badge/STATUS-EXTERNAL_TOOL_MIGRATION-025E91?style=for-the-badge" alt="Status: external-tool migration" /> <img src="https://img.shields.io/badge/TARGET-WINDOWS_10%2F11_X64-025E91?style=for-the-badge" alt="Windows 10/11 x64" /> <img src="https://img.shields.io/badge/REQUIRED_SUBSCRIPTIONS-%240-025E91?style=for-the-badge" alt="No mandatory subscription" /></p>

> **November 18, 2026 product decision and engineering update:** The owner has directed the Suite to **replace the old OTS-built detection engines** (RepoScan, W.H.I.T.E., System Sanitizer and Firewall79X) with reviewed external tools. **The new 0.10.0 source builds a third-party-only Windows hub; the previously published 0.9.x installers still launch the retired RepoScan engine and must not be used as the new product. Full upstream scanner integration remains incomplete.** Earlier green CI runs and test-prerelease installers relate to the former implementation, not the new target stack. Current decisions: [Tool register](docs/TOOLING_DECISIONS.md), [migration roadmap](docs/IMPLEMENTATION_ROADMAP.md), [documentation index](docs/README.md). Production release remains **NO-GO**.

### What the new app actually does

The `src/OTSSecuritySuite.Hub/` Windows app has a tab for every selected third-party project, shows official vendor sources, connects allowlisted local Windows executables (with PE header checks), opens genuine Windows Security and installed KopiaUI windows, and displays local `--help` for installed upstream CLI tools. It does **not** reimplement their detectors, bundle vendor binaries, run security scans, automatically install anything or call a paid API. The older OTS scanners remain in Git history for recovery but are **not shipped** by the 0.10.0 hub installer. [Current Windows CI evidence](https://github.com/ohanatechsolutions/OTS-Security-Suite/actions/runs/44470904063) · [Security CI evidence](https://github.com/ohanatechsolutions/OTS-Security-Suite/actions/runs/44470904447). An unsigned short-lived test artifact exists; manual Windows acceptance, true tool execution and production release gates remain open.

---

## <img src="art/readme-icons/why.svg" width="24" height="24" alt="" /> Why This Exists — Problem, Purpose & Solution

| Problem | Purpose | Solution |
|---|---|---|
| Antivirus, threat hunting, vulnerability checks, hardening, backup and security auditing are scattered across unrelated tools. Reimplementing them duplicates logic and risks blind spots. | Make the real coverage, limits, and current health of each security tool accessible from one Windows application. | Use a minimal launcher/orchestrator around vetted external engines, with consistent reporting, per-tool consent, scoped scanning and evidence-based release gates. |

## <img src="art/readme-icons/stack.svg" width="24" height="24" alt="" /> Selected External Tool Roster

These are **selected external tools with navigation in the new hub preview**; the hub does not bundle their executables or provide complete vendor scan integrations. Native vendor programs open separately and CLI vendors can show only their local help. The precise source URLs, cost/license constraints, acceptance criteria and implementation state for all 14 tools are in [TOOLING_DECISIONS.md](docs/TOOLING_DECISIONS.md).

| Role | Target tools | What actually exists now |
|---|---|---|
| Existing endpoint antivirus and file signatures | **Microsoft Defender**, **ClamAV**, **YARA-X** | Defender passive status exists; ClamAV/YARA-X adapters not built |
| Endpoint inventory and memory investigation | **osquery**, **PE-sieve** (optional) | Planned, not integrated |
| Windows event investigation and hardening | **Hayabusa**, **HardeningKitty**, **Sysmon** (optional) | Planned, not integrated |
| Optional encrypted backup/restore | **Kopia** | Planned, not integrated |
| Repository credentials, source and dependency risks | **TruffleHog**, **Semgrep CE**, **OSV-Scanner** | CI checks configured; desktop adapters not built |
| AI agent skill security | **NVIDIA SkillSpector** | Conditional shared CI check when skills exist |
| Authorized autonomous penetration test | **Strix OSS CLI** (optional, locally hosted model) | Opt-in WPF adapter and synthetic tests; real Docker/model network-sandbox validation not done |

**Related DevSecOps checks:** Shared CI also uses Trivy and language-specific tests/lint. Branch protection, optional isolated OWASP ZAP, a defensible agent sandbox strategy (possibly NVIDIA OpenShell), build provenance, and installer SHA-256 checks are requirements, **not automatic Suite tabs**.

**Excluded from the future product:** OTS RepoScan, OTS W.H.I.T.E., OTS System Sanitizer, OTS Firewall79X, LOKI-RS, paid AI APIs, cloud-only scanners and mandatory subscriptions. Keep historical sources, release assets and any original quarantine/backup data intact. The Suite may keep a custom **UI shell**; the detection engines should be upstream reviewed tools.

## <img src="art/readme-icons/workflow.svg" width="24" height="24" alt="" /> Intended Safe Workflow

1. Check that the selected upstream tool is authentic, correctly versioned and compatible with the local Windows environment.
2. Confirm ownership or authorization of a narrowly scoped target before analysis.
3. Run read-only checks first and keep Windows Defender/Firewall in control of their own protection.
4. Report actual completed, partial, unsupported and failed states; do not present a tool crash as a clean result.
5. Require explicit operator authorization before service installation, telemetry, hardening changes, restore, quarantine, deletion or autonomous tests.
6. For Strix, use a disposable authorized target with separately verified Docker network containment; a loopback LLM endpoint by itself is not a sandbox.

## <img src="art/readme-icons/roadmap.svg" width="24" height="24" alt="" /> Honest Build Status

| Workstream | State | Evidence / next gate |
|---|---|---|
| Prior WPF host | **VERIFIED historical preview** | Previous Windows CI build/smoke checks only; still depends on retired RepoScan |
| Sanitizer product removal | **IMPLEMENTED in current host** | Unshipped old module; historical source and vaults preserved |
| Strix local adapter | **SCOPED TESTS PASS; runtime BLOCKED** | Synthetic local-target/credential-copy tests; real Strix/Docker isolation not tested |
| New external-tool runtime integrations | **NOT COMPLETE** | Integrate each official engine with license, fail-closed contracts, authentic Windows tests |
| Universal DevSecOps | **CONFIGURED / verify latest runs** | Shared scanner/test workflow and OSV; no blanket PASS claim |
| Full new product and production installer | **NO-GO** | Independent Windows 10/11 tests, licensing, security, artifacts and owner release approval pending |

**Implementation priority:** [M0–M8 roadmap](docs/IMPLEMENTATION_ROADMAP.md) begins with replacing old RepoScan dependencies without deleting historical data, then adds genuine upstream tools one at a time.

## <img src="art/readme-icons/installation.svg" width="24" height="24" alt="" /> Source & Development

The existing repository is a **historical integration preview**, not a working installer containing the selected 14 external tools. To inspect its present source:

    git clone https://github.com/ohanatechsolutions/OTS-Security-Suite.git
    cd OTS-Security-Suite

Use the existing [Windows build workflow](.github/workflows/windows-build.yml) only for scoped legacy-shell verification. Its success does **not** certify the new architecture. Do not install an older test prerelease expecting ClamAV, osquery or the other new tools.

- **[Tool register](docs/TOOLING_DECISIONS.md):** current selection, explicit rejections, upstream links, status, license/cost and traceability.
- **[Implementation roadmap](docs/IMPLEMENTATION_ROADMAP.md):** M0–M8 stages and measurable acceptance gates.
- **[Documentation index](docs/README.md):** current versus historically superseded records.
- **[Project status](docs/PROJECT_STATUS.md)** · **[Test evidence](docs/TEST_EVIDENCE.md)** · **[Release gates](docs/RELEASE-GATES.md):** verified historical checks and current blockers.
- **[Strix safety](docs/STRIX_INTEGRATION.md)** · **[Dependency inventory](docs/DEPENDENCY_INVENTORY.md):** previous adapter and currently built dependencies.

**Cost and legal safety:** No mandatory paid services or APIs. Upstream free/open-source code is not automatically cleared for binary/rule redistribution; every integrated version and rules license needs review. Hardware, Windows licensing, optional storage and power are separate. Never silently elevate privileges, disable Defender, download arbitrary executables, run unauthorized pentests or delete old user data. No new production release is approved.

---

## <img src="art/readme-icons/acknowledgements.svg" width="24" height="24" alt="" /> Acknowledgements

Security engines belong to their respective upstream maintainers; retain their identity, versions, licenses, copyrights and third-party notices. OTS provides the controlled Windows integration surface and honest results, not claims to have reimplemented the tools.

<p align="center"><sub>Developed by <strong>Ohana Tech Solutions</strong>. Project accent: <strong>#025E91</strong>.</sub></p>
