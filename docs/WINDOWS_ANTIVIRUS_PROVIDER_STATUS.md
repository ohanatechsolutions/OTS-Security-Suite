# Windows antivirus provider discovery — requirement WIN-AV-001

**Status:** Implemented on review branch; real Windows acceptance pending.
**Scope:** Windows 10/11 desktop client. Read-only detection; no installation, security setting mutation, antivirus disabling, or download. Future scanner integration is separate work.

## Product behavior

- On startup and when opening Windows Security, query registered antivirus providers in Windows Security Center using PowerShell Get-CimInstance from root/SecurityCenter2.
- Independently query Get-MpComputerStatus for Microsoft Defender and classify it active only if real-time and antivirus states are enabled with Normal running mode; a passive state is **not** claimed as active.
- Display registered product names (such as AVG or Norton) with ON / OFF / UNKNOWN. When one product is reported active, show **REAL-TIME ANTIVIRUS REPORTED ON** rather than guaranteeing the machine is secure. Multiple antivirus registrations may represent outdated state; users can verify in Windows Security.
- When all identified providers report OFF, show a warning on first launch. When no provider is returned by a healthy Security Center query, show **NO ANTIVIRUS REGISTERED**, not “no antivirus is installed.”
- When Security Center and Defender cannot be queried, or provider flags cannot be interpreted, show **UNKNOWN**; do not silently equate a query failure to being unprotected.
- Open the official built-in Windows Security user interface via windowsdefender: or ms-settings:windowsdefender. Microsoft Defender Antivirus is already bundled with Windows 10 and 11; **do not advertise an external download**.
- Launching the Windows Security settings view is user-approved; this feature makes no automatic changes.
- The Microsoft Defender Quick Scan button is available only when Defender is independently confirmed active; no button for scanning via unsupported AVG/Norton vendor APIs.
- The existing on-demand suite inspection remains available even when an AV status query fails; the protection banner describes any uncertainty.

## Security and limitations

The Security Center productState bytes are a compatibility heuristic rather than a documented stable public status contract. Recognize only known on/off encodings; unknown encodings remain unknown. A malicious or unregistered security product, a stale provider record, antivirus update failures, tampering or compromised Windows Security Center can cause misclassification. “Reported ON” means only the local platform reported an active engine; it is not a proof of clean system or effective protection.

The query uses a fixed noninteractive PowerShell command, bounded to 12 seconds, does not perform network access, and does not read or transmit antivirus secret settings. No elevation is requested. Windows Server, macOS and Linux support are N/A in this Windows-only iteration. Windows hardening, ClamAV, YARA-X, LOKI-RS, osquery, Kopia, Hayabusa, Sysmon, and PE-sieve adapters are **NOT IMPLEMENTED by this change**.

## Acceptance tests

- WIN-AV-001-A: Microsoft Defender normal active; third-party product off or passive Defender must not be mislabeled.
- WIN-AV-001-B: Registered third-party product on, Defender passive => on status for third-party and disabled Defender scan button.
- WIN-AV-001-C: Registered third-party product off => attention warning and Open Windows Security action.
- WIN-AV-001-D: No registered products with functioning WSC => no registered antivirus warning, no external download instruction.
- WIN-AV-001-E: WSC unavailable and no Defender status => unknown, not falsely unprotected.
- WIN-AV-001-F: Windows 10/11 on-device real provider compatibility, Windows Security activation and startup prompt behavior => manual acceptance **PENDING**.
- WIN-AV-001-G: Compile and execute audit smoke tests on Windows CI => verify after PR run.

**Release gate:** No deployment, publication, installer-signing, third-party security package installation, or retirement of the original Sanitizer is authorized by this change.
