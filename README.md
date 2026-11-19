<p align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="art/SecuritySuite-header-dark.png" />
    <source media="(prefers-color-scheme: light)" srcset="art/SecuritySuite-header-light.png" />
    <img src="art/SecuritySuite-header-light.png" width="100%" alt="OTS Security Suite original branded banner" />
  </picture>
</p>

# OTS Security Suite

<p align="center"><strong>One workspace for repository inspection, Windows security, and authorized threat assessment.</strong></p>

<p align="center">An integrated Windows desktop security utility by <strong>Ohana Tech Solutions</strong>, combining RepoScan, read-only Windows protection visibility, and an optional local Strix autonomous testing adapter. System Sanitizer is excluded from the current product.</p>

<p align="center">
  <img src="https://img.shields.io/badge/STATUS-INTEGRATION_PREVIEW-025E91?style=for-the-badge" alt="Status: integration preview" />
  <img src="https://img.shields.io/badge/PLATFORM-WINDOWS_10%2F11_X64-025E91?style=for-the-badge" alt="Platform: Windows 10 and 11 x64" />
  <img src="https://img.shields.io/badge/STACK-.NET_10_%7C_GO_%7C_PYTHON-025E91?style=for-the-badge" alt="Stack: .NET 10, Go, and Python" />
</p>

> **Release status:** This is an **engineering preview, not an approved public release**. The recorded [Windows integration workflow](https://github.com/ohanatechsolutions/OTS_Security_Suite/actions/runs/44406324294) succeeded for the antivirus coexistence update, but independent Windows 10/11 acceptance, publisher code signing, historical user-vault restoration and other [release gates](docs/RELEASE-GATES.md) remain open. An earlier [unsigned owner-only draft test prerelease](https://github.com/ohanatechsolutions/OTS_Security_Suite/releases/tag/untagged-8b649701cd0ffb13c62d) exists; it is **not** the current main-branch release.

---

## <img src="art/readme-icons/why.svg" width="24" height="24" alt="" /> Why This Exists

Repository inspection, source auditing, Windows protection checks and authorized penetration testing are easier to oversee from one workspace. OTS Security Suite keeps detection methods distinct: the routine repository scanner is read-only, while the optional Strix agent can execute code and therefore requires an explicit, separate authorization. System Sanitizer and general cleanup are no longer included in the application.

<table>
  <tr>
    <td align="center" width="33%">
      <img src="art/readme-icons/problem.svg" width="20" height="20" alt="" />
      <p><strong>The Problem</strong></p>
      <p>Repository scanners, endpoint status checks and dynamic penetration testing use different interfaces. Users need clear boundaries between a heuristic report and an autonomous security assessment.</p>
    </td>
    <td align="center" width="34%">
      <img src="art/readme-icons/purpose.svg" width="20" height="20" alt="" />
      <p><strong>The Purpose</strong></p>
      <p>Provide one Windows entry point for authorized security inspection while making incomplete results, tool availability and execution risks clear.</p>
    </td>
    <td align="center" width="33%">
      <img src="art/readme-icons/solution.svg" width="20" height="20" alt="" />
      <p><strong>The Solution</strong></p>
      <p>A native WPF host embeds RepoScan, displays Windows antivirus and network posture, and offers an optional locally hosted Strix assessment against a disposable copy of an authorized code directory.</p>
    </td>
  </tr>
</table>

---

## <img src="art/readme-icons/workflow.svg" width="24" height="24" alt="" /> How It Works

1. **Select a module.** Choose RepoScan, Windows Security, Network Defense or the optional Strix tab.
2. **Supply an authorized target.** Routine RepoScan scans supported repository URLs or local source files without running their code.
3. **Review coverage.** RepoScan returns heuristic findings and clear incomplete states; Windows status is read-only.
4. **Opt in to Strix separately.** Select an installed Strix Windows CLI, an owned local folder and a numeric-loopback local model endpoint. Explicitly confirm ownership and execution risks.
5. **Run inside a disposable source copy.** The adapter limits copy size, launches headless quick mode with a zero external API budget, and offers cancellation. Docker network isolation is not guaranteed.
6. **Inspect local results.** Strix reports may contain sensitive data. The copied source directory is removed after the run; no automatic remediation, cloud uploads, or broad internet testing is offered.

### Integrated modules

| Module | Present capability | Important boundary |
| :--- | :--- | :--- |
| **RepoScan · Repository Security** | Go scanner for supported local inputs and public GitHub/Bitbucket source snapshots; 95 heuristic detection rules across 12 reported categories | Heuristics and inventory coverage are not proof of complete malware detection |
| **Windows Security** | Read-only antivirus protection status and direct Windows Security access | Does not replace Defender or include the former System Sanitizer |
| **Network Defense** | Read-only Windows Firewall inspection, TCP connection inventory and selected security-event analysis | Never silently changes firewall rules |
| **Strix · optional** | Explicitly installed external Strix CLI, scoped local-source copy, loopback self-hosted LLM, headless assessment, cancellation and local reports | Not bundled, not auto-run, Docker network isolation not assured; Windows/CLI acceptance pending |
| **Legacy W.H.I.T.E. code audit** | Replacement Python command-line auditor used internally in CI | Not shipped as a user-facing module; offline CVE/SCA unavailable |
| **Chrome extension** | Manifest V3 popup for passing supported repository targets to the local app | Requires registered `ots-security://` protocol |

Windows Security Center integration can display registered antivirus protection status, including unavailable or inactive states, without installing or modifying antivirus products. See [AV-001 implementation evidence](docs/AV_001_ANTIVIRUS_COVERAGE.md).

### Architecture

```text
OTS Security Suite — native Windows WPF / WebView2 host
├── RepoScan → Go scanner · ephemeral 127.0.0.1 child service
├── Windows Security → built-in Windows Security Center status
├── Network Defense → read-only firewall/events/TCP inventory
├── Strix (optional, user-installed CLI)
│   ├── explicit authorization + numeric loopback local model
│   ├── bounded disposable copy of owned local source
│   └── external Docker-based agent; local run reports
└── Chrome MV3 extension → ots-security://
```

Additional design details are maintained in the [build blueprint](docs/BUILD_BLUEPRINT.md).

---

## <img src="art/readme-icons/stack.svg" width="24" height="24" alt="" /> Tech Stack

| Component | Technology | Description |
| :--- | :--- | :--- |
| Desktop shell | <img src="https://skillicons.dev/icons?i=cs,dotnet" height="22" alt="C# and .NET" /> **C# / .NET 10 / WPF** | Native Windows interface, process coordination and host integration |
| Embedded scanner view | **Microsoft WebView2** | Displays the loopback-only repository scanner interface |
| Repository analysis | <img src="https://skillicons.dev/icons?i=go" height="22" alt="Go" /> **Go 1.27.4+** | Local scanning engine, rule evaluation and bounded inspection |
| Code auditing | <img src="https://skillicons.dev/icons?i=python" height="22" alt="Python" /> **Python 3.12** | Local audit CLI and structured redacted results |
| Optional pentest adapter | **Strix CLI (external) / Docker / local LLM** | User-installed, opt-in integration with a capped source snapshot; not bundled |
| Browser integration | <img src="https://skillicons.dev/icons?i=js,chrome" height="22" alt="JavaScript and Chrome" /> **Chrome Manifest V3 / JavaScript** | Optional local suite launcher |
| Packaging | **Inno Setup 6 / PyInstaller** | Windows installer and bundled Python executable |
| Engineering validation | <img src="https://skillicons.dev/icons?i=githubactions" height="22" alt="GitHub Actions" /> **GitHub Actions** | Recorded Windows integration builds and smoke checks |

**Licensing:** The repository does not currently provide a verified root distribution license. Component-specific license and third-party notices must be honored. Do not assume the complete suite is MIT-licensed merely because incorporated components carry their own licenses.

---

## <img src="art/readme-icons/installation.svg" width="24" height="24" alt="" /> Installation & Usage

### Supported development target

- **OS target:** Windows 10/11 x64; independent clean-device compatibility acceptance is still pending.
- **Build prerequisites:** .NET 10 SDK, Go 1.27.4 or newer compatible toolchain, Python 3.12, PyInstaller, Inno Setup 6 and Microsoft WebView2 Runtime.
- **Historical source:** System Sanitizer files remain in the repository temporarily for provenance and recovery, but are not referenced by the shipped desktop application or installer. No Git submodules are required.
- **Distribution:** An older unsigned draft test installer exists for the owner. There is **no signed, publicly approved production release**.

### Get the source

```powershell
git clone https://github.com/ohanatechsolutions/OTS_Security_Suite.git
cd OTS_Security_Suite
```

### Build the Windows integration preview

From the repository on GitHub, use **Actions → Windows build and integration checks → Run workflow** where available and permitted. That workflow compiles the WPF host and Go engine, validates CI-only Python audit logic, and produces an Inno Setup test installer. The Strix executable and System Sanitizer are not bundled.

For detailed environment, provenance, verification and packaging requirements, consult the [build blueprint](docs/BUILD_BLUEPRINT.md), [test evidence](docs/TEST_EVIDENCE.md) and [release gates](docs/RELEASE-GATES.md). Do not assume an older draft installer corresponds to the latest `main` commit.

### Run selected developer checks

```bash
(cd scanner && go test ./... && go vet ./...)
python -m unittest discover -s tools -v
node --check extension/popup.js
```

These are **component checks**, not a substitute for a Windows desktop build, clean install, GUI acceptance or security review.

### First use and safety

Routine RepoScan analysis is non-executing. The optional Strix adapter instead runs an autonomous security agent against a temporary copy of a local source directory and requires an explicit per-run confirmation. Strix needs Docker and a local AI model; a local AI endpoint does not prevent the Docker sandbox from making other network requests. Use only an owned test project on a disposable, unprivileged machine. No Strix software or model is downloaded by the Suite.

Never treat a clean heuristic result or a Strix run without validated findings as proof of safety. Keep the existing Windows antivirus enabled. The old Sanitizer vault and its data are not migrated or deleted; use the separate historical recovery procedures if you have prior quarantined files.

Unsigned development builds may produce Microsoft SmartScreen or antivirus reputation warnings; signing and reputation verification remain open release work.

---

## <img src="art/readme-icons/roadmap.svg" width="24" height="24" alt="" /> Roadmap & Verification

Checked items refer to **implemented functionality with the repository's recorded scoped engineering evidence**, not certification or complete release readiness.

- [x] **Native integration shell:** WPF desktop host and WebView2 scanner integration built and exercised in recorded Windows CI.
- [x] **Repository inspection:** Go scanner and 95-rule inventory covered by recorded automated tests.
- [x] **Local audit preview:** Redacted Python report workflow with explicit incomplete result states.
- [x] **Legacy source preservation:** Original Sanitizer 42-file source tree remains archived in this repository, but is no longer a product module on the feature branch.
- [x] **Antivirus coexistence indicator:** Windows Security Center status handling covered by recorded [AV-001 CI](docs/AV_001_ANTIVIRUS_COVERAGE.md).
- [x] **Historical Windows packaging evidence:** Previous unsigned Inno test installer and synthetic checks passed on older code, not this feature branch.
- [ ] **Strix integration:** New opt-in code and regression tests pending Windows CI, installed-CLI verification, and independent sandbox review.
- [ ] **Sanitizer removal regression:** New installer must be checked to contain neither the Sanitizer executable nor its navigation.
- [ ] **Independent Windows acceptance:** Clean Windows 10/11 installation, actual DPI, taskbar/icon and accessibility checks.
- [ ] **Historical data safeguard:** Preserve legacy quarantine/vault data outside the new installation; no deletion or migration is authorized.
- [ ] **Browser acceptance:** Clean-profile Chrome extension testing and normal install/uninstall behavior.
- [ ] **W.H.I.T.E. capability parity:** Reconcile unrecovered historical detection behavior and missing offline dependency CVE/SCA.
- [ ] **Repository retirement clearance:** Independently archive the legacy Sanitizer Git history and security-held draft release before the owner considers deletion.
- [ ] **Public release gates:** Root license decision, distribution review, publisher signing, security acceptance and owner go/no-go approval.

**Repository retirement warning:** The former OTS W.H.I.T.E. repository is inaccessible; its original source history has not been recovered into this suite. The legacy System Sanitizer source has been incorporated, but its old repository is **not yet safe to delete**. Follow the [retirement runbook](docs/SANITIZER_REPO_RETIREMENT_2026-11-18.md) and preserve its historical refs and unpublished held assets separately.

**Current handoff:** [Project status](docs/PROJECT_STATUS.md) · [Test evidence](docs/TEST_EVIDENCE.md) · [Owner finish line](docs/OWNER_FINISH_LINE.md) · [Release gates](docs/RELEASE-GATES.md)

---

## <img src="art/readme-icons/acknowledgements.svg" width="24" height="24" alt="" /> Acknowledgements

This suite ships Ohana Tech Solutions repository scanning code; historical System Sanitizer source remains preserved but is not part of the current application distribution. The optional Strix integration uses a separately installed, user-maintained third-party CLI under its own Apache-2.0 license. The replacement W.H.I.T.E. auditor is new in-repository code, **not** a restored copy of the deleted historical codebase. Third-party components, including those packaged with the scanner and Windows tooling, retain their respective legal notices; review the repository's component notices and [source provenance](docs/REPOSITORY_CONSOLIDATION_2026-11-18.md) before redistribution.

---

<p align="center">
  <sub>Developed by <b>Ohana Tech Solutions</b>.</sub>
</p>
