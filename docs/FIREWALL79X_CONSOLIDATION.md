# OTS Firewall79X → OTS Security Suite: consolidation register

**Task:** CONSOL-F79X-001 · **Date:** 2026-11-18 · **Source:** `ohanatechsolutions/OTS_Firewall79X@8b0996b22fe53b3cb820a3b76492941070619a87` · **Target baseline:** `ohanatechsolutions/OTS_Security_Suite@49f088550f6983c63969622accc1da1932321689`

## Owner decision and safe destination

The requested product direction is to **retire the standalone Firewall79X product only after its genuinely useful functionality has been migrated and verified in OTS Security Suite**. The original 79 numbered items are *requirements*, not proven active firewall mechanisms; their unchanged names are retained below for traceability.

- OTS Security Suite is the **only future consumer application / installer** for these features; Windows Defender Antivirus and Windows Firewall remain the authoritative base protection.
- The **Network Defense** tab owns firewall-profile health and bounded TCP inventory. Its first implementation is **read-only**, never requiring elevated permissions or disabling Windows protection, and never installing a second firewall driver.
- Existing Suite RepoScan, WHITE replacement and Sanitizer continue to own repository/code scanning, system malware/Defender integration, cleanup and user quarantine. Do not duplicate their engines.
- Do **not** ship the old privileged Firewall79X service, custom temporary firewall rule engine, hosts-file writer or automatic hardening alongside Suite until narrow functionality has passed a separate threat model, permissions design, negative tests and real Windows compatibility testing.
- Security Suite has other **open antivirus-coexistence PRs (#6 and #7)**. Reconcile them before making this network view the authoritative whole-device Defender health display. Do not merge duplicated health services.
- Both repositories remain accessible during migration. **No repo/branch deletion, no history rewrite, no automatic new signed/public release, and no transfer of old users' protected logs or keys**.

## Unique value found in Firewall79X source

1. Windows Firewall profile audit, default inbound action and rule-overlap analysis (`WindowsSettings.cs`, `FirewallOverlap.cs`, `LayerChecks.cs`).
2. Native connection and endpoint inventory, with a separate distinction between **observation** and **blocked by OTS** (`NetworkTables.cs`, `EventSensors.cs`, `BlockedConnectionEvent.cs`).
3. Conditional network intrusion indicators: inbound port scans and blocked-connection windows, DNS rebinding patterns, unusual reconnection/beaconing, unsafe Wi-Fi and local network changes. Coverage depends on Windows event audit policy and sensors.
4. Durable protected event logging and integrity checking with separate service/GUI boundary (`LocalStore.cs`, `EventIntegrity.cs`, `MachineLogKeyRing.cs`). A safe migration needs permissions, privacy, key continuity and rollback design—not blind duplication.
5. Narrow extra network restrictions, feeds, canaries and hardening. Some are unfinished, can affect connectivity or introduce privileged code, and **are explicitly excluded from the initial consumer merge**.

## Implementation and verification

**Source changes in the consolidation PR:** `src/OTSSecuritySuite.Desktop/NetworkProtectionAudit.cs`, `MainWindow.xaml`, `MainWindow.xaml.cs`, network regression cases in `tests/OTSSecuritySuite.AuditSmoke/`.

- Read-only COM access to Windows Firewall domain/private/public enabled and default inbound policy.
- Bounded read-only TCP connection inventory (max 250 displayed); no DNS lookup, packet capture, direct network traffic, firewall rule changes, remote log collection, or persistence.
- Unknown/unavailable policy states are explicitly shown as unverified, never as protected.
- A UI tab offers refresh and opens native Windows Security. No PowerShell requirement for consumers.
- Initial regression cases verify healthy, disabled, default-allow and unavailable states. Windows build/installed GUI smoke and physical Windows 10/11 acceptance **must still run**.
- No claim of complete layer 1, 16 or 19 protection: these are **small observation slices**, not the original full layer specifications.

## Additional safe consolidation slice: firewall rule overlap hints

Migrated the original MIT-licensed `FirewallOverlap.cs` analyzer into the Suite as a bounded **advisory-only** utility. An explicit user button reads up to 801 enabled Windows Firewall rules and reports duplicate, opposing and potentially overlapping conditions with all the limitations of the original heuristic. The UI never changes firewall policies and never claims computed Windows Filtering Platform / Group Policy effective precedence. Synthetic regression tests exercise duplicates, contradictory action, broad inbound allow rules and malformed ports. Independent Windows 10/11 usability and rule-provider compatibility remain NOT RUN; source/CI evidence must be attached after verification.

## Requirements crosswalk (79 of 79)

`Existing Suite module` means *related functionality exists*, **not** original requirement parity. All full-spec certifications remain unverified.

| ID | Original Firewall79X requirement | Proposed owner | Current migration/evidence |
|---:|---|---|---|
| 1 | Default-Deny Inbound Firewall | Network Defense module | Read-only first slice only; full original layer unverified |
| 2 | Rate Limiting and Flood Protection | Network Defense module | Not migrated / original requirement not certified |
| 3 | Port Scan Detection | Network Defense module | Not migrated / original requirement not certified |
| 4 | Anti-Spoofing and Malformed Packet Filtering | Network Defense module | Not migrated / original requirement not certified |
| 5 | Threat-Intelligence IP Blocking | Network Defense module | Not migrated / original requirement not certified |
| 6 | IPv6 Protection Parity | Network Defense module | Not migrated / original requirement not certified |
| 7 | Cloud Metadata Endpoint Protection | Network Defense module | Not migrated / original requirement not certified |
| 8 | DNS Rebinding Protection | Network Defense module | Not migrated / original requirement not certified |
| 9 | Untrusted Network Protection | Network Defense module | Not migrated / original requirement not certified |
| 10 | Bluetooth Security Monitoring | Network Defense module | Not migrated / original requirement not certified |
| 11 | Honeytokens and Canary Files | Suite shared security/services | Not migrated / original requirement not certified |
| 12 | Canary DNS and URLs | Suite shared security/services | Not migrated / original requirement not certified |
| 13 | Decoy Services and Honeypots | Suite shared security/services | Not migrated / original requirement not certified |
| 14 | Intrusion Detection | Network Defense module | Not migrated / original requirement not certified |
| 15 | Network Metadata and Attack Classification | Network Defense module | Not migrated / original requirement not certified |
| 16 | Connection and DNS Journal | Network Defense module | Read-only first slice only; full original layer unverified |
| 17 | ARP Spoofing Alerts | Network Defense module | Not migrated / original requirement not certified |
| 18 | Rogue DHCP and Evil-Twin Wi-Fi Detection | Network Defense module | Not migrated / original requirement not certified |
| 19 | Listening Port and Attack-Surface Monitor | Network Defense module | Read-only first slice only; full original layer unverified |
| 20 | New LAN Device Alerts | Network Defense module | Not migrated / original requirement not certified |
| 21 | Unsafe Network Alerts | Network Defense module | Not migrated / original requirement not certified |
| 22 | Suspicious SMTP Activity | Network Defense module | Not migrated / original requirement not certified |
| 23 | Remote-Access Monitoring | Network Defense module | Not migrated / original requirement not certified |
| 24 | Clock Drift Monitoring | Network Defense module | Not migrated / original requirement not certified |
| 25 | Persistence and Software Installation Monitoring | Existing Suite module (parity audit) | Not migrated / original requirement not certified |
| 26 | Rootkit and Kernel Driver Monitoring | Suite shared security/services | Not migrated / original requirement not certified |
| 27 | Privilege Escalation Monitoring | Suite shared security/services | Not migrated / original requirement not certified |
| 28 | Camera and Microphone Access Alerts | Suite shared security/services | Not migrated / original requirement not certified |
| 29 | Cryptominer and Resource-Abuse Detection | Suite shared security/services | Not migrated / original requirement not certified |
| 30 | Runtime and System Activity Monitoring | Suite shared security/services | Not migrated / original requirement not certified |
| 31 | Browser Credential and Session Protection | Suite shared security/services | Not migrated / original requirement not certified |
| 32 | Clipboard Hijacking Detection | Suite shared security/services | Not migrated / original requirement not certified |
| 33 | System Trust Configuration Monitoring | Suite shared security/services | Not migrated / original requirement not certified |
| 34 | Bulk-Copy and Exfiltration Alerts | Suite shared security/services | Not migrated / original requirement not certified |
| 35 | File and Binary Integrity Monitoring | Existing Suite module (parity audit) | Not migrated / original requirement not certified |
| 36 | Suspicious Execution Monitoring | Existing Suite module (parity audit) | Not migrated / original requirement not certified |
| 37 | Boot-Chain Integrity Monitoring | Suite shared security/services | Not migrated / original requirement not certified |
| 38 | Keylogger and Input-Hook Detection | Suite shared security/services | Not migrated / original requirement not certified |
| 39 | Package and Software Source Integrity | Existing Suite module (parity audit) | Not migrated / original requirement not certified |
| 40 | Container Exposure Audit | Suite shared security/services | Not migrated / original requirement not certified |
| 41 | Brute-Force Protection | Network Defense module | Not migrated / original requirement not certified |
| 42 | SSH Hardening Audit | Suite shared security/services | Not migrated / original requirement not certified |
| 43 | Credential Breach Checking | Suite shared security/services | Not migrated / original requirement not certified |
| 44 | Account Security Audit | Suite shared security/services | Not migrated / original requirement not certified |
| 45 | Login and Privilege-Use Alerts | Network Defense module | Not migrated / original requirement not certified |
| 46 | Windows Security Hardening Audit | Suite shared security/services | Not migrated / original requirement not certified |
| 47 | Reversible Automatic Hardening | Suite shared security/services | Not migrated / original requirement not certified |
| 48 | Browser Security Audit | Suite shared security/services | Not migrated / original requirement not certified |
| 49 | External Exposure Assessment | Suite shared security/services | Not migrated / original requirement not certified |
| 50 | Secrets Hygiene Scanner | Existing Suite module (parity audit) | Not migrated / original requirement not certified |
| 51 | Remote Feature and Hardware Security Audit | Suite shared security/services | Not migrated / original requirement not certified |
| 52 | Updates and Vulnerability Assessment | Existing Suite module (parity audit) | Not migrated / original requirement not certified |
| 53 | Watchdog Service | Suite shared security/services | Not migrated / original requirement not certified |
| 54 | Dead-Man's Switch | Suite shared security/services | Not migrated / original requirement not certified |
| 55 | Privilege Separation | Suite shared security/services | Not migrated / original requirement not certified |
| 56 | Hostile Input Handling | Suite shared security/services | Not migrated / original requirement not certified |
| 57 | Verified Security Updates and Feeds | Suite shared security/services | Not migrated / original requirement not certified |
| 58 | Supply-Chain Protection | Suite shared security/services | Not migrated / original requirement not certified |
| 59 | Rule Safety Guard | Suite shared security/services | Not migrated / original requirement not certified |
| 60 | Alert Pipeline | Suite shared security/services | Not migrated / original requirement not certified |
| 61 | Secure Structured Logging | Suite shared security/services | Not migrated / original requirement not certified |
| 62 | Resource Governor | Suite shared security/services | Not migrated / original requirement not certified |
| 63 | Threat Feed Freshness | Suite shared security/services | Not migrated / original requirement not certified |
| 64 | Protected Secrets Storage | Suite shared security/services | Not migrated / original requirement not certified |
| 65 | UI and Privileged Service Access Protection | Suite shared security/services | Not migrated / original requirement not certified |
| 66 | Forensic Snapshot | Suite shared security/services | Not migrated / original requirement not certified |
| 67 | Security Posture Score | Suite shared security/services | Not migrated / original requirement not certified |
| 68 | Encrypted Versioned Backups | Suite shared security/services | Not migrated / original requirement not certified |
| 69 | Offline Recovery Kit | Suite shared security/services | Not migrated / original requirement not certified |
| 70 | Resilient Log Export | Suite shared security/services | Not migrated / original requirement not certified |
| 71 | Automated Regression Tests | Suite shared security/services | Not migrated / original requirement not certified |
| 72 | Quarterly Security Reports | Suite shared security/services | Not migrated / original requirement not certified |
| 73 | Firewall Conflict and Shadowing Analysis | Network Defense module | Advisory analyzer migrated; full effective Windows policy precedence NOT certified |
| 74 | Security Documentation Generator | Suite shared security/services | Not migrated / original requirement not certified |
| 75 | Known-Malicious Domain Blocking | Network Defense module | Not migrated / original requirement not certified |
| 76 | Download and Temporary File Malware Scanning | Existing Suite module (parity audit) | Not migrated / original requirement not certified |
| 77 | USB and Removable Device Alerts | Suite shared security/services | Not migrated / original requirement not certified |
| 78 | Ransomware Behavior Monitoring | Suite shared security/services | Not migrated / original requirement not certified |
| 79 | Beaconing Detection | Network Defense module | Not migrated / original requirement not certified |

## Gates before archiving or deleting the original repository

- [ ] Validate exact source snapshot, tests, tags, issues, releases, license notices and any protected user data locations; preserve an **independent full Git mirror and checksum** and restoration instructions.
- [ ] Migrate every intentionally retained feature; compare behavior against the source's specification and test fixtures. Mark any intentionally dropped function with rationale and owner approval, not as completed.
- [ ] Reconcile the Suite's open #6/#7 Defender/coexistence PRs and verify no redundant protection services and no security toggles that disable native Windows protection.
- [ ] Windows 10 and Windows 11 independently tested for installer, no-admin read-only view, firewall profiles, network inventory, Defender coexistence, offline behavior, shutdown/uninstall and preserved user data.
- [ ] CI, signature/provenance, secret/license/security scans and release gates pass for the **actual integrated main build**. Historical Firewall79X security-scan issues remain unresolved.
- [ ] Owner confirms post-merge product behavior and explicitly approves archiving/deleting original repository **after** all archives and rollback routes are tested.

**Decision:** Retain and develop Security Suite; Firewall79X source is a controlled migration reference, not a second installable engine. Repo retirement is **BLOCKED**, not complete.
