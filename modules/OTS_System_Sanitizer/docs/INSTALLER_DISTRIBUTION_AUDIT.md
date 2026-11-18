# Installer distribution migration evidence

- Date: 2026-11-15 (Hawaii); automated GitHub Actions in UTC 2026-11-16.
- Status: **ARCHIVED IN RESTRICTED DRAFT — PUBLICATION BLOCKED**.
- Original tracked asset: `installer/OTSSystemSanitizerSetup-x64.exe`; size 46,476,112 bytes.
- SHA-256: `6aee66341f0cb5870be9832e04029f9fc5d524e63dc387d7c2bbc323c90bb358`.
- Archive: [v0.4.0-legacy-security-hold](https://github.com/ohanatechsolutions/OTS_System_Santizer/releases/tag/v0.4.0-legacy-security-hold), **DRAFT / NOT PUBLIC**; only repository maintainers can access it.
- [Archive workflow run 43796247312](https://github.com/ohanatechsolutions/OTS_System_Santizer/actions/runs/43796247312): **SUCCESS** — executable MZ and PE headers, architecture, checksum, draft upload and byte-for-byte asset download verified.
- [Historical binary review 41708669808](https://github.com/ohanatechsolutions/OTS_System_Santizer/actions/runs/41708669808): previous **SUCCESS**, limited to static header and recognizable pattern checks.
- [Publication-security scan 41706727937](https://github.com/ohanatechsolutions/OTS_System_Santizer/actions/runs/41706727937): **FAILED**, potential secret-pattern findings in reachable history. Publication is BLOCKED pending triage and remediation.
- The original installer and checksum were removed from the `main` source tree **only after** their verified copy was preserved in the nonpublic draft.
- Build workflow and `.gitignore` prevent future compiled EXEs from being committed into the source tree.
- Still **NOT RUN**: security finding remediation, fresh publication scan on latest revision, clean Windows installation and GUI acceptance, independent malware/signer checks, final production signoff.
- Binary remains in historical Git commits and this restricted draft; no destructive history rewrite was performed.
- Other project phases (noninstaller code changes): **N/A** — this task moved existing distributables without changing application features.
