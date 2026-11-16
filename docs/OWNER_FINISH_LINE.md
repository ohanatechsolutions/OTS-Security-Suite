> **2026-11-15 — VERIFIED owner-only test prerelease:** [GitHub Windows Actions #43820859841](https://github.com/ohanatechsolutions/OTS_Security_Suite/actions/runs/43820859841) **SUCCESS** (both Windows build and draft prerelease jobs), source commit 953f1f3037cb4a2f8781c95ae8823370db5ef086. The owner-visible [DRAFT v0.9.0-test.2 prerelease](https://github.com/ohanatechsolutions/OTS_Security_Suite/releases/tag/untagged-8b649701cd0ffb13c62d) contains a **real unsigned** OTS-Security-Suite-Setup-x64.exe (102,419,492 bytes, genuine MZ PE) and SHA256SUMS.txt. The authenticated GitHub Release API independently confirms draft=true, prerelease=true, both assets and installer digest sha256:f672384e94c26c97664a368dfba98fafcb52c17b62fb0ead4a3ad11ae7841dd2. The Windows job verified Go, 95 rules, 8/8 new Python WHITE tests, 22/22 .NET tests, WPF UI automation, Inno installation, repair/uninstall and synthetic quarantine preservation. **Only a TEST PREVIEW is available; independent Windows10/11 acceptance, genuine legacy vault QA, Chrome clean-profile, old WHITE detection parity, offline SCA, code signing and public release approval remain NOT VERIFIED / NO-GO.** The older test.1 draft is superseded. Further draft release creation is manual-only, with a unique tag, to prevent unintended repeat publication or CI costs.

> **2026-11-15 owner decision / new build candidate:** The owner deleted the standalone OTS W.H.I.T.E. repository and directed the suite to proceed independently. The previous WHITE Git submodule is **not** part of the new preview candidate. A new in-repository Python heuristic auditor replaces its CLI contract, but **full legacy WHITE parity and offline dependency CVE analysis are NOT VERIFIED / NOT AVAILABLE**. The new candidate must pass its own Windows CI before an unsigned **draft** test prerelease may be created. Previous successful CI and installer hashes apply only to historical submodule builds. Main and original Sanitizer remain unchanged. See [WHITE replacement scope](../white_engine/NOTICE.md) and [test preview handoff](TEST_PRE_RELEASE.md).

# OTS Security Suite — Owner Finish Line

**Status:** Verified engineering preview; **NO-GO for signed public release** until the real-machine and publisher gates below. This is the short handoff: the repository contains the real code, tests, installer, security policy, and release tooling. No paid services, public release, branch deletion, or private-key access were performed.

## What engineering has already verified

**Last verified engineering-code commit:** `ba7562b41e7ec1abda69406fc0b1a235993e3df7` — [Windows CI #43414634076](https://github.com/ohanatechsolutions/OTS_Security_Suite/actions/runs/43414634076) **SUCCESS**, Windows runner. Actual logs: Go tests/vet and scanner build PASS; Chrome popup contracts **13/13 PASS**; embedded RepoScan URL normalization **17/17 PASS**; offline W.H.I.T.E. source/history audit **172 worktree files scanned, zero high/critical findings reported by the configured audit** (not an independent security certification); packaged WHITE checks PASS; 22/22 Windows C# process/report/quarantine tests PASS; Sanitizer tests PASS; native WPF publish PASS; Inno Setup package PASS; install/WPF UI Automation/second-launch handoff/normal close/repair/uninstall and synthetic user-vault retention PASS; source/submodule/compiler provenance manifest PASS. **Unsigned CI test installer SHA-256: `826DD311F59CAD0E4BEB7A2F1353921CD5C790D90577D73B5C99C40FF4C830AA`**. Artifact upload intentionally skipped on PR, no public signed release. Independent real-device QA, historical DPAPI recovery, Chrome clean-profile, publisher signing and final owner approval remain **NOT RUN/BLOCKED**.

You do **not** need to edit or rewrite the code. The remaining steps below are real-machine acceptance, the licensing/signing decision, and explicit permission for any public release. A **new manually dispatched run** has its own checksum—do not reuse `826DD311F59CAD0E4BEB7A2F1353921CD5C790D90577D73B5C99C40FF4C830AA` for that later artifact.

## 1 — Download a private-use UNSIGNED test build

1. Open [Windows build and integration checks](https://github.com/ohanatechsolutions/OTS_Security_Suite/actions/workflows/windows-build.yml).
2. Choose **Run workflow** and select branch **work/white-audit-lifecycle-and-blueprint-20261111**. This manual action is the only workflow event allowed to upload test installer artifacts. Do not select an old commit or build.
3. When the run is green, open **Artifacts** and download **OTS-Security-Suite-Windows-Beta** (seven-day retention). This is **not a signed or approved public release**. GitHub repository visibility can expose Actions metadata/artifacts, so do not treat it as a secure private distribution platform.
4. Extract the installer and SHA256SUMS.txt. Compare the SHA-256 of the installer to the checksum **in the same workflow run**. Two unsigned builds can have different binary hashes; do not compare against a historical PR hash.

Run the included read-only verification script on Windows:

    .\tools\verify-windows-release.ps1 -InstallerPath .\OTS-Security-Suite-Setup-x64.exe -ExpectedSha256 YOUR_64_CHARACTER_HASH

An Authenticode warning is expected for an unsigned test build. Do **not** disable Defender or Windows security checks to suppress it.

## 2 — Complete actual Windows 10/11 acceptance

Use disposable VMs or spare machines where possible, and **standard-user sessions**, not everyday production computers. Back up existing encrypted user quarantine/DPAPI context before any legacy compatibility experiment. Use only harmless synthetic sample files for quarantine/deletion testing.

- On Windows 11 and Windows 10 x64, verify launch, close, uninstall and same-version reinstall; confirm no existing quarantine or reports are erased.
- Inspect the real white-on-transparent logo, title bar and taskbar icon, Alt+Tab identity, 1300×800 default where practical, and usable layout at **100%, 150%, 200% scaling**. Test keyboard focus/navigation and accessibility.
- Run an offline local RepoScan folder audit. Review partial/uninspected counts. Cancel one harmless long scan. Check that no old report is offered as a successful completed scan.
- Use a synthetic Git repository in W.H.I.T.E. and test normal run, incomplete history mode, export, cancellation and closing the suite during an active audit; confirm no child process remains.
- Test Windows Security with read-only inspections and Defender installed/unavailable/denied outcomes. Do not change protected systems or permissions to force a pass.
- Test quarantine/restore using one synthetic user-profile file and a **backed-up copy** of genuine legacy Aegis Prime vault data *only* in its authorized original user/DPAPI context. Verify cleanup never deletes unselected or recently changed files.
- In a clean Chrome test profile, load the reviewed unpacked extension directory (Chrome extensions page, Developer Mode). Confirm activeTab-only permission, open desktop action, current repository handoff, restart/revocation, and that no unexpected external data is uploaded.

Run this non-destructive Windows diagnostic for each tested OS:

    .\tools\collect-windows-qa.ps1 -OutputPath .\OTS-Windows-QA-TestComputer.json

The output stays on that computer and records OS version/build, system DPI, installed app hash/signature and URI-handler status, without private filenames, account names or passwords. **It does not complete the human visual, Chrome, legacy-vault or Defender tests.** Review the JSON before sharing it.

Record actual PASS/FAIL, test date, OS edition/build, installer SHA, sanitized evidence and reviewer in [QA_ACCEPTANCE.md](QA_ACCEPTANCE.md).

## 3 — Provide an actual Ohana Tech Solutions publisher identity

An installer Publisher string and a SHA-256 file **do not substitute for an Authenticode signature**. Obtain/approve a legitimate OTS code-signing certificate and a trusted timestamp URL if you intend to distribute binaries. This may cost money; no certificate has been purchased or used. **Never give ChatGPT, GitHub, or anyone in a support chat a PFX, private key, signing password or key export.**

The source includes these release tools:

- tools/sign-windows-release.ps1 — use on a trusted Windows SDK signing machine with a legitimate certificate already in the local user store. Sign the bundled executables **before** Inno Setup and the installer **after** packaging. It validates signatures and regenerates the checksum.
- tools/verify-windows-release.ps1 — verify the final PE and same-build hash, and require the trusted OTS signature and timestamp for public release.
- docs/RELEASE_RUNBOOK.md — full signing, rollback and release steps.

This signing operation is **not automatic in CI** and is not authorized until the real certificate and publisher decision are available.

## 4 — Approve the actual release and root repository license

Review the residual risks and approve the suite's root source-code distribution license. Complete product, security, Windows QA and release-owner signoffs. Only then approve any merge to main, signed versioned GitHub Release, publisher/store submission, or public download.

The following tracks are N/A to the first local Windows edition because they do not exist in its V1 requirements: cloud SaaS, mobile app, LLM agent, subscriptions, hardware/firmware and hosted account synchronization.

**Nothing above requires rewriting the original RepoScan, W.H.I.T.E. or Sanitizer applications. Their source and historical encrypted data remain preserved.** All outstanding work is documented, rather than marking it verified without evidence.
