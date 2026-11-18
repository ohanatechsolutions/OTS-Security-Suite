# AV-001 — Windows antivirus coexistence and protection-status integration

Date: 2026-11-18 (HST). State: **Implementation in review; independent device acceptance NOT VERIFIED.**

## Requirements and decisions

- Query the supported Windows Security Center COM `IWSCProductList` interface for registered antivirus products, product state (On, Off, Snoozed, Expired) and signature currency.
- Distinguish active/current antivirus (**PROTECTED · antivirus status only**), active/outdated (**ATTENTION**), installed but off/snoozed/expired (**UNPROTECTED**), no registered antivirus (**UNPROTECTED**), and unreadable/partial inventory (**UNKNOWN**). Avoid claiming the device is malware-free.
- Preserve any current antivirus choice: OTS does not install, activate, deactivate or replace Norton, AVG, Defender or another product.
- When no antivirus is registered, direct the user to **Windows Security** to activate or repair the built-in Microsoft Defender Antivirus on Windows 10/11; never suggest third-party Defender downloads.
- A Defender quick scan may be requested only when WSC reports Defender active. Other audit tools remain user-operated; a protection warning is not silently bypassed.
- Show the status in the existing Windows Security tab; refresh on startup, tab navigation and button action. No telemetry is sent remotely.

## Risk and limitations

Windows Security Center reports *registration and current status*, not an independent adversarial integrity guarantee. Registered product names are provider-supplied display strings. Users should still consult Windows Security and their AV provider. A failure to contact WSC yields UNKNOWN and no affirmative safety claim. WSC support is intended for interactive Windows desktop, not Windows Server or managed endpoints where the API may be unavailable. Do not use undocumented WMI `productState` bit masks.

Scope exclusion: automatic installation, force-enabling protection, switching default AV, firewall changes, macOS/Linux, and optional third-party scanner integration — N/A for the AV-001 slice.

## Verification plan and evidence

- `dotnet run --project tests/OTSSecuritySuite.AntivirusSmoke/OTSSecuritySuite.AntivirusSmoke.csproj -c Release` — 16 status-matrix assertions, synthetic product inputs; executed by Windows CI.
- `dotnet publish src/OTSSecuritySuite.Desktop/OTSSecuritySuite.Desktop.csproj -c Release -r win-x64 --self-contained true` — compile WPF + COM interop.
- Windows user acceptance remains required: Defender active, third-party active (Defender passive), installed/off, stale definitions, no registered AV, WSC unavailable; confirm Windows Security launch and scan behavior with and without admin rights.
- No standalone Windows 10/11 physical-machine verification claimed; no installer/release published for this slice.

Reference: Microsoft IWSCProductList, IWscProduct, WSC_SECURITY_PRODUCT_STATE and Microsoft Support Defender built-in Windows 10/11 guidance.
