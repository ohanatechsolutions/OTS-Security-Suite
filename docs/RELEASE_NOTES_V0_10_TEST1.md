# OTS Security Suite v0.10.0-test.1 — Third-Party Cybersecurity Hub

**Unsigned, owner-only engineering test preview. Not a production-ready security product.**

This test pre-release replaces the rejected 0.9.x OTS-built scanner interface with a third-party-tool launcher and per-tool navigation. The new executable does **not** bundle RepoScan, OTS W.H.I.T.E., OTS System Sanitizer, Firewall79X, or the old Chrome extension. Historic source, Git branches, user settings and legacy vaults are intentionally preserved.

## Included

- Windows 10/11 x64 per-user installer of a self-contained .NET 10 native WPF hub.
- Navigation for the 14 approved external cybersecurity tools: Microsoft Defender, ClamAV, YARA-X, osquery, KopiaUI, Hayabusa, HardeningKitty, Sysmon, PE-sieve, TruffleHog, OSV-Scanner, Semgrep CE, NVIDIA SkillSpector and optional Strix.
- Opens **actual Windows Security**; if separately installed and connected, opens the **genuine KopiaUI window**. A connected upstream CLI may show its bounded `--help` output. Other manual/privileged tools link to their official documentation.
- Clear tool connection states, explicit local .exe selection with allowlisted filename and PE header checks; no automatic third-party installs, scans, updates, privilege changes or paid APIs.
- Real Windows CI upgrade test: installs the SHA-256-pinned 0.9.0-test.5 legacy installer, then installs 0.10.0 over it and checks old engines are removed and synthetic user-data/vault canaries survive. This verifies those particular canaries, **not a full real-user DPAPI vault restoration**.

## Test instructions

1. Test on a Windows 10/11 x64 VM or spare PC. Keep backups of the previously installed application and any sensitive data.
2. Verify SHA-256 with `Get-FileHash .\OTS-Security-Suite-Setup-x64.exe -Algorithm SHA256` against the attached `SHA256SUMS.txt`.
3. Exit any old OTS Security Suite process and install using standard Windows security prompts. The installer is **not Windows publisher-signed**; **do not disable Defender or SmartScreen** to run it.
4. Launch and confirm old RepoScan/WHITE/Sanitizer/Firewall79X tabs are absent and the 14 upstream tool entries are present.
5. Use **Windows Security**, then optionally connect a known, already-installed **KopiaUI.exe** and verify it opens with its genuine vendor UI.
6. Try an uninstalled program, a wrong filename and a valid installed CLI `--help` path, and verify no background scan starts.
7. Test resize/DPI and closing/relaunching, then uninstall and verify personal data is retained.

## Known limits / production NO-GO

This release is a **launcher preview**, not finished end-to-end integrations of all 14 upstream engines. CLI security scans, privilege-requiring tools, and autonomous Strix runs are **not wired**. No vendor executable, signature database, Docker image or local model is bundled. The project still requires real-device Windows 10/11 accessibility checks, actual upstream runtime/compatibility verification, installer upgrade testing with real customer data, individual license review, Authenticode signing and explicit owner signoff before production.

**Scope:** User-authorized QA pre-release only; no fees, branch deletions or irreversible cleanup.
