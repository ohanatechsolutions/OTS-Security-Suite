# OTS Security Suite — Jarvis Incident Response Track

**Date:** 2026-11-20 (HST)  
**Status:** IN PROGRESS — first intake slice implemented; **NOT DEPLOYED**  
**Base inspected:** `main@1da5de24275b387145d88c10733b5783f9e4ed23`  
**Source of truth:** existing `docs/THIRD_PARTY_HUB_DECISION.md`, `docs/TOOLING_DECISIONS.md`, `docs/PROJECT_STATUS.md`. This track extends the vendor-tool hub; it does not restore retired proprietary scanner engines.

## Hardware and cost decisions

- Intel NUC 12 Enthusiast: 32 GB **system RAM**, Intel Arc A770M **16 GB VRAM** (VRAM is not general-purpose RAM for security services); Windows/Jarvis/AI workloads share system resources.
- Firewalla Orange: intended perimeter and inter-VLAN enforcement; API/alert export and license/cost availability **unverified**.
- Raspberry Pi 3 B+: isolated OpenCanary sensor; no access to NUC management, storage or Firewalla admin.
- ICY BOX IB-RD3820 RAID 1 DAS: local redundant storage/backup target; independent protected backup still required.
- **UPS deferred by owner for funding.** UPS agent, NUT, power-failure shutdown automation: **N/A in this build — not purchased**. Revisit when funded.
- No paid subscriptions, cloud AI transfer of incident data, automatic software installs, production deployment, releases, or destructive actions authorized.

## Architecture and trust boundaries

```text
ISP -> Firewalla -> VLAN-capable switch -> NUC (Jarvis/Windows agents)
                                   \-> Pi (OpenCanary decoy, isolated)
NUC --USB--> ICY BOX RAID DAS --separate protected versioned backup--> offsite/offline
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
| IR-009 | Versioned encrypted backups and tested clean restore | **NOT STARTED** | Kopia selected upstream; real storage and restore UAT required |
| IR-010 | Attack simulations and Windows 10/11 / Pi 3 / Firewalla end-to-end verification | **NOT RUN** | Hardware unavailable in this development environment |
| IR-011 | UPS monitoring and shutdown | **N/A — deferred by owner** | Funding and UPS model not selected |

### Risk register

| Risk | Mitigation / release gate |
|---|---|
| False positives cause outage | No automatic network-wide lockdown; start alert-only; staged allowlisted containment and rollback tests |
| Compromised honeypot becomes pivot | Separate VLAN, least-privilege access, no management/data route, hardened Pi |
| Incident logs contain secrets | Metadata-only schema; no raw logdata; local ACLs, retention and deletion policy before deployment |
| Monitoring fails with NUC outage | Firewalla and Pi remain independent; external alert route to be designed |
| Ransomware reaches attached DAS | Separate immutable/append-only or offline backup; restore test; don't snapshot contaminated data as clean |
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

- **E-IR-001:** 2026-11-20, Linux Python 3.13.5; `python -m unittest discover -s tests -v` in isolated local copy of `incident_center/`: **4 tests passed**. Covered malformed and oversized records, dedupe, metadata-only storage, CLI and limit validation.
- **E-IR-002:** `python -m compileall -q incident_center.py tests`: **passed** in isolated local copy.
- **NOT RUN:** exact GitHub branch clean-clone verification, GitHub Actions on branch, Windows integration, Pi live sensor, Firewalla live integration, restore test, human UAT, installer/release. Production remains **NO-GO**.

## Next smallest safe slice

1. Add an authenticated, one-way OpenCanary export mechanism with explicit schema/version, replay protections and test fixtures. Do not expose SQLite or an unauthenticated HTTP listener.
2. Integrate incident read-only display into the **current** WPF hub, preserving existing vendor launcher behavior.
3. Validate Windows ACLs, retention, timestamp formats and privacy redaction.
4. Only then add Firewalla and Defender event adapters and alert-only correlation.
5. Design containment and backup protection as separately authorized, reversible actions with an operator kill switch.

**Applicability:** mobile-app release, billing, cloud hosting, payment compliance: **N/A — local Windows security hub**. Installer QA, Windows privilege boundaries, supply-chain licensing, endpoint isolation and backup recovery: **required, pending**.
