# Watchdog — ICF Identity & Account-Takeover Protection

**Decision date:** 2026-11-20 HST  
**Status:** ARCHITECTURE ACCEPTED / IMPLEMENTATION NOT STARTED / PRODUCTION NO-GO  
**Scope:** ICF employee Google identities, Google Drive/Sheets/Forms/Gmail, Squarespace, TanDM, Jarvis, pCloud and privileged OTS operations. This is a cross-platform policy and application-control layer, **not** a new firewall appliance or a claim that SaaS accounts are already secured.

## Goal and threat model

**Primary threat:** An attacker obtains an employee's password, session cookie, OAuth grant, recovery email access, or authorized device, then uses legitimate access to steal data, modify vendor records, send fraudulent communications, or pivot into Jarvis and administrative systems.

**Security objective:** Compromise of one employee identity should not automatically confer owner privileges, global data export, destructive administrative access, control of backup credentials, or network administration. No architecture can guarantee zero loss of data that the employee is legitimately permitted to read.

**Trust boundaries:**
1. Identity provider sign-in does not itself authorize Jarvis or TanDM operations.
2. Jarvis/TanDM must enforce role, resource ownership, session validity, and approval on the server for every privileged operation.
3. Google, Squarespace and pCloud retain their own native permissions and recovery mechanisms; Watchdog cannot override platform controls.
4. Firewalla/VLANs control network paths, not authorization within Google/Squarespace.
5. Incident alerts and account suspension must not depend solely on the potentially compromised employee mailbox.

## Access policy (proposed, not yet enforced)

| Role | Business records | Messaging | Security/network admin | Bulk export, account deletion, financial changes |
|---|---|---|---|---|
| Owner / break-glass admin | Necessary privileged access via separate account | Administrative | Explicit owner-only | Step-up authentication, independent approval when feasible, audit |
| Employee | Minimum assigned folders/sheets; controlled workflows | Assigned TanDM conversations | Denied | Denied by default |
| Jarvis automation service | Scoped service identity and minimal data access | Explicitly approved actions only | No general admin | Denied without owner approval |
| Security sensor / Pi | Metadata-only incident output | None | No management network route | Denied |

Do not assume Google personal Gmail accounts provide organization-wide suspension, audit logs, or managed device controls. **First determine whether each employee account is Google Workspace-managed, personal Gmail, or mixed**; implement only the controls actually supported by the account tier. Do not promise any paid Workspace features for free.

## Required controls and acceptance tests

| ID | Control | Acceptance / test evidence needed | Status |
|---|---|---|---|
| WD-001 | Inventory identities, Google account type, SaaS admins, OAuth grants and recovery channels | Owner-approved account/privilege matrix; no secrets stored in repo | NOT STARTED |
| WD-002 | Phishing-resistant MFA for privileged identities; MFA/passkeys for staff | Enrollment and recovery tests, including lost-device case | NOT STARTED |
| WD-003 | Separate owner and recovery accounts; independent protected pCloud recovery access | Simulated employee account loss cannot reset owner/backup admin credentials | NOT STARTED |
| WD-004 | Least-privilege Google sharing, Squarespace roles, TanDM and Jarvis scopes | Employee cannot modify master vendor data, security settings or other employee resources without explicit rights | NOT STARTED |
| WD-005 | Server-side Jarvis/TanDM authorization and step-up for destructive/bulk actions | Automated deny tests for stolen employee session, wrong role, expired/revoked session, direct API calls | NOT STARTED |
| WD-006 | Alert on suspicious changes where APIs/plan permit | Test detection and independent owner notification for new forwarding rules, unexpected shares and privilege changes | NOT STARTED |
| WD-007 | Incident response and emergency revocation | Drill: suspend/revoke where supported, rotate tokens, inspect forwarding/sharing, preserve audit evidence, restore access | NOT STARTED |
| WD-008 | Minimal encrypted, versioned recovery to existing pCloud | Restore critical Jarvis configs and a sample business export after simulated deletion; verify retention and credentials isolation | NOT STARTED |
| WD-009 | Privacy/retention and cost audit | No employee content surveillance without approved policy; no paid subscriptions introduced | NOT STARTED |

## First implementation sequence

1. **Read-only discovery:** inventory account types, current owners/admins, current sharing and available audit capabilities. Do not collect passwords, MFA recovery codes or access tokens in documentation.
2. **Owner hardening:** isolate owner/admin and recovery identities; enable phishing-resistant authentication and test recovery paths.
3. **Employee least privilege:** narrow Drive/Sheets/Squarespace permissions and adopt controlled workflows for vendor approvals.
4. **Jarvis/TanDM security implementation:** role and resource authorization on the backend, privileged-action approval, short-lived/revocable sessions and audit trails.
5. **Alert-only detection and drills:** confirm available Google/Squarespace sign-in/change signals, independent notifications and a tested revocation runbook.
6. **Recovery test:** minimal pCloud protected backups only for irreplaceable data. Plain synchronization is not equivalent to a versioned backup.

## Safety and scope

- **No changes made** to Google, Squarespace, TanDM, Jarvis, Firewalla, pCloud or employee accounts by this documentation update.
- Do not automatically suspend employees or block entire networks based on a single anomaly; false positives can interrupt business.
- No secret/token values, customer emails, live IP addresses, or vendor personal data belong in GitHub issues, test fixtures or this document.
- UPS, DAS and dedicated RAID backup: **N/A for the first hardware release — owner deferred**. Minimal recoverability of critical local data: **required, unverified**.
- OpenCanary is the Pi honeypot. OpenCanopy is an optional display layer; it does not replace the honeypot.
- Mobile apps, multi-tenant SaaS billing and cloud AI transfer: **N/A — internal ICF/Jarvis deployment**.

## Evidence and release gate

**Evidence:** Architecture decision recorded only. No employee permission audit, MFA enrollment, simulated account takeover, live API integration, Windows installer, CI on this change, or hardware verification has been performed.  
**Release gate:** BLOCKED until WD-001 through WD-008 applicable tests are passed, account-specific limitations are documented, and owner acceptance is recorded.
