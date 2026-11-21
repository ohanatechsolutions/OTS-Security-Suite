# OTS Security Suite — Jarvis Incident Response Track

**Date:** 2026-11-20 (HST)  
**Status:** IN PROGRESS — first intake slice implemented; **NOT DEPLOYED**  
**Base inspected:** `main@1da5de24275b387145d88c10733b5783f9e4ed23`  
**Source of truth:** existing `docs/THIRD_PARTY_HUB_DECISION.md`, `docs/TOOLING_DECISIONS.md`, `docs/PROJECT_STATUS.md`. This track extends the vendor-tool hub; it does not restore retired proprietary scanner engines.

## Owner-approved five-layer system architecture (2026-11-20)

| Name | Planned component | Responsibility | Current verification |
|---|---|---|---|
| **Brain** | Intel NUC 12 Enthusiast / Serpent Canyon (32 GB system RAM, Arc A770M 16 GB VRAM, 1 TB SSD) | Jarvis, Windows security telemetry, OTS Security Suite incident center | Hardware choice accepted; runtime load and physical setup not tested |
| **Barbed Fence** | Firewalla **Orange** (exact product/SKU pending verification) | Perimeter, VPN and inter-VLAN policy where supported | Hardware choice accepted; actual features, licensing, API/export and deployment not verified |
| **Booby Trap** | Raspberry Pi 3 B+ with **OpenCanary** | Isolated decoy services and alert source | Planned; Pi setup and log contract not tested |
| **Secured Pathway** | **Managed, VLAN-capable** LAN switch | Physical connectivity and VLAN transport; switch does not itself guarantee secure traffic | Model, 802.1Q support, VLAN policies and physical configuration pending |
| **Watchdog** | Identity-centric zero-trust policies across Google, Squarespace, Jarvis, TanDM and administrator accounts | Phishing-resistant MFA, least privilege, separate admin identity, authorization, high-risk action approval, alerts, revocation | **Architecture approved; controls NOT deployed or verified** |

**Naming clarification:** OpenCanary is the honeypot service. OpenCanopy is a separate optional web dashboard for OpenCanary events, **not** the decoy service itself. Dashboard deployment and exposure are not authorized.

**Scope and cost:** Owner deferred the UPS, ICY BOX DAS, and two 4 TB drives for the initial release. Use the existing 1 TB NUC SSD; existing pCloud 2 TB may hold a **minimal, encrypted, versioned recovery copy** of irreplaceable local configurations/databases and critical business exports, subject to integration and restore validation. No additional cloud purchase approved. RAID/DAS-based backup is **N/A — deferred**, but recovery of critical data is **NOT waived**. No production backup automation has been deployed.

**Safety boundaries:** Do not expose Pi decoys, OpenCanopy, the NUC management interface or SQLite incident storage directly to the internet. A honeypot hit is a signal, not proof of compromise. No automated Firewalla blocking, mass account suspension, external alerts or employee permission changes until separately tested and authorized. The NUC can fail; ensure administrative access to network and identity systems remains independently recoverable.

## Architecture and trust boundaries

```text
Internet/ISP -> Barbed Fence (Firewalla) -> Secured Pathway (managed VLAN switch)
                                         |-> Brain (NUC 12 / Jarvis + Security Suite)
                                         |-> Booby Trap (Pi 3 B+ / OpenCanary on isolated VLAN)
Watchdog: separate identity/authorization policies at Google, Squarespace,
          Jarvis, TanDM and admin entry points (not a device in the LAN path).
Minimal protected recovery: critical local files -> encrypted versioned pCloud
                              (integration and restore NOT YET VERIFIED)
```

- **Decoy access is not proof of a Firewalla breach.** Treat an OpenCanary interaction as a signal for investigation.
- Pi produces OpenCanary JSONL; operator transfers approved log export to a trusted local path. The first slice reads that file **without opening a network port** and stores only time, source/destination IP, logtype, a deduplication hash, and severity `review`. Raw `logdata` including credentials is **not stored**.
- SQLite is local and should be kept on a protected volume with appropriate Windows ACLs. The tool sets restrictive creation permissions on POSIX, but **Windows ACL validation is NOT RUN**.
- The intake is not connected to the shipped WPF hub, does not send notifications, does not monitor files continuously and never executes containment commands.

## Requirement and acceptance tracker

| ID | Requirement / acceptance | Status | Evidence or blocker |
|---|---|---|---|
| IR-001 | Parse local OpenCanary JSONL; reject invalid records; do not persist credentials | **IMPLEMENTED, locally tested** | `incident_center/incident_center.py`; `tests/test_incident_center.py` |
| IR-002 | Re-import does not create duplicate incidents | **IMPLEMENTED, locally tested** | SQLite unique fingerprint and regression test |
| IR-003 | Display recent incident metadata locally | **IMPLEMENTED, locally tested** | CLI `recent` |
| IR-004 | Secure Pi-to-NUC transport, authentication, source authenticity and replay handling | **NOT STARTED** | Needs threat model, signed/authenticated transport, VLAN and Pi deployment |
| IR-005 | Correlate Defender, Sysmon, Firewalla and OpenCanary with timestamp normalization | **NOT STARTED** | Firewalla API/export and Windows log contracts unverified |
| IR-006 | Local dashboard, case history and operator acknowledgements | **NOT STARTED** | WPF hub integration required |
| IR-007 | Out-of-band alerts and escalation | **NOT STARTED** | Need channel, credential, availability and privacy approval |
| IR-008 | Containment playbooks with explicit allowlist, dry-run, rollback, approval, audit trail | **NOT STARTED** | No automatic firewall or endpoint changes permitted yet |
| IR-009 | Minimal encrypted versioned recovery copies to existing pCloud, with tested restore | **NOT STARTED** | DAS deferred; verify pCloud integration, separation of credentials, retention and restore before production |
| IR-010 | Attack simulations and Windows 10/11 / Pi 3 / Firewalla end-to-end verification | **NOT RUN** | Hardware unavailable in this development environment |
| IR-011 | UPS monitoring and shutdown | **N/A — deferred by owner** | Funding and UPS model not selected |
| ID-001 | Watchdog account inventory, account types and privilege map | **NOT STARTED** | Determine personal Gmail vs Workspace; list owners/admins and service recovery paths |
| ID-002 | Phishing-resistant MFA and protected independent admin identities | **NOT STARTED** | Enrollment and recovery UAT required for each platform |
| ID-003 | Server-side least privilege in Jarvis/TanDM; step-up approval for destructive/export actions | **NOT STARTED** | Authz tests for employee, compromised session and admin required |
| ID-004 | Account compromise detection, alert routing and session/token revocation runbook | **NOT STARTED** | Platform capabilities and emergency drills unverified |

### Risk register

| Risk | Mitigation / release gate |
|---|---|
| False positives cause outage | No automatic network-wide lockdown; start alert-only; staged allowlisted containment and rollback tests |
| Compromised honeypot becomes pivot | Separate VLAN, least-privilege access, no management/data route, hardened Pi |
| Incident logs contain secrets | Metadata-only schema; no raw logdata; local ACLs, retention and deletion policy before deployment |
| Monitoring fails with NUC outage | Firewalla and Pi remain independent; external alert route to be designed |
| Account theft or ransomware corrupts critical records | Restrict privileges, separate owner/admin credentials, maintain encrypted versioned recovery in independent pCloud account, test restore; do not treat ordinary sync as backup |
| Firewalla automation requires paid API | Manual operator-approved rules unless a free supported interface is verified |
| Heavy security services impact local AI | Baseline NUC RAM/CPU while AI uses GPU; defer full Wazuh/Velociraptor servers pending benchmark |

## Run the first local slice (Python 3.10+)

From repository root:

```bash
python -m unittest discover -s incident_center/tests -v
python incident_center/incident_center.py --db ./incidents.sqlite import /path/to/opencanary.log
python incident_center/incident_center.py --db ./incidents.sqlite recent --limit 20
```

**Privacy:** Never commit `incidents.sqlite`, OpenCanary logs or production IPs. Keep test data synthetic. Windows file ACLs must be configured before importing real events. The command processes existing local logs only; it does **not** connect to Firewalla, Pi or Jarvis.

## Evidence (not release certification)

- **E-IR-001:** 2026-11-20, Linux Python 3.13.5; `python -m unittest discover -s tests -v` in isolated local copy of `incident_center/`: **5 tests passed**. Covered malformed and oversized records (including multi-chunk lines), dedupe, metadata-only storage, CLI and limit validation.
- **E-IR-002:** `python -m compileall -q incident_center.py tests`: **passed** in isolated local copy.
- **NOT RUN:** exact GitHub branch clean-clone verification, GitHub Actions on branch, Windows integration, Pi live sensor, Firewalla live integration, restore test, human UAT, installer/release. Production remains **NO-GO**.

## Next smallest safe slice

1. Add an authenticated, one-way OpenCanary export mechanism with explicit schema/version, replay protections and test fixtures. Do not expose SQLite or an unauthenticated HTTP listener.
2. Integrate incident read-only display into the **current** WPF hub, preserving existing vendor launcher behavior.
3. Validate Windows ACLs, retention, timestamp formats and privacy redaction.
4. Only then add Firewalla and Defender event adapters and alert-only correlation.
5. Define Watchdog platform-specific permissions, identity threat model and incident-revocation drill; test independently of the firewall.
6. Design containment and minimal pCloud recovery as separately authorized, reversible actions with an operator kill switch.

**Applicability:** mobile-app release, billing, cloud hosting, payment compliance: **N/A — local Windows security hub**. Installer QA, Windows privilege boundaries, supply-chain licensing, endpoint isolation and backup recovery: **required, pending**.
