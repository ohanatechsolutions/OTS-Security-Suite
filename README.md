<p align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="art/SecuritySuite-header-dark.png" />
    <source media="(prefers-color-scheme: light)" srcset="art/SecuritySuite-header-light.png" />
    <img src="art/SecuritySuite-header-light.png" width="100%" alt="OTS Security Suite original branded banner" />
  </picture>
</p>

# OTS Security Suite

<p align="center"><strong>One workspace for repository inspection, Windows security, and controlled system cleanup.</strong></p>

<p align="center">An integrated Windows desktop security utility by <strong>Ohana Tech Solutions</strong>, combining a repository scanner, a local code-and-secrets audit preview, and System Sanitizer functionality.</p>

<p align="center">
  <img src="https://img.shields.io/badge/STATUS-INTEGRATION_PREVIEW-025E91?style=for-the-badge" alt="Status: integration preview" />
  <img src="https://img.shields.io/badge/PLATFORM-WINDOWS_10%2F11_X64-025E91?style=for-the-badge" alt="Platform: Windows 10 and 11 x64" />
  <img src="https://img.shields.io/badge/STACK-.NET_10_%7C_GO_%7C_PYTHON-025E91?style=for-the-badge" alt="Stack: .NET 10, Go, and Python" />
</p>

> **Release status:** This is an **engineering preview, not an approved public release**. The recorded [Windows integration workflow](https://github.com/ohanatechsolutions/OTS_Security_Suite/actions/runs/44406324294) succeeded for the antivirus coexistence update, but independent Windows 10/11 acceptance, publisher code signing, historical user-vault restoration and other [release gates](docs/RELEASE-GATES.md) remain open. An earlier [unsigned owner-only draft test prerelease](https://github.com/ohanatechsolutions/OTS_Security_Suite/releases/tag/untagged-8b649701cd0ffb13c62d) exists; it is **not** the current main-branch release.

---

## <img src="art/readme-icons/why.svg" width="24" height="24" alt="" /> Why This Exists

Repository inspection, code auditing and Windows cleanup are easier to oversee when their results and safety controls are accessible from one desktop workspace. OTS Security Suite brings together existing Ohana Tech Solutions security utilities without pretending that different inspection techniques are interchangeable. Its design emphasizes local processing, transparent scan limitations and deliberate approval before destructive actions, while maintaining the original component source and specialist capabilities.

<table>
  <tr>
    <td align="center" width="33%">
      <img src="art/readme-icons/problem.svg" width="20" height="20" alt="" />
      <p><strong>The Problem</strong></p>
      <p>Separate repository, source-audit and Windows maintenance tools require different workflows. Users can lose sight of scan coverage, incomplete checks or the consequences of acting on a suspicious finding.</p>
    </td>
    <td align="center" width="34%">
      <img src="art/readme-icons/purpose.svg" width="20" height="20" alt="" />
      <p><strong>The Purpose</strong></p>
      <p>Provide one Windows entry point for inspecting repositories and local system conditions, keeping specialist functions available while separating detection and investigation from actions that change or delete files.</p>
    </td>
    <td align="center" width="33%">
      <img src="art/readme-icons/solution.svg" width="20" height="20" alt="" />
      <p><strong>The Solution</strong></p>
      <p>A native WPF host coordinates a Go repository scanner, a Python audit preview and vendored System Sanitizer functionality. It presents findings and supports review-led quarantine, restoration and selected cleanup.</p>
    </td>
  </tr>
</table>

---

## <img src="art/readme-icons/workflow.svg" width="24" height="24" alt="" /> How It Works

1. **Select a module.** Choose Repository Security, Code & Secrets Audit, Windows Security or Space Recovery in the native desktop interface.
2. **Supply an authorized target.** Enter a public repository URL or select a local folder/file for supported scanning; Windows security and cleanup operations work on the local machine.
3. **Run bounded analysis.** The Go repository engine operates as a locally hosted child process; the Python auditor produces redacted results, and the Sanitizer .NET core handles supported Windows inspections.
4. **Review the outcome.** Inspect findings and incomplete states. A heuristic warning does not establish that a file is infected; not all files receive the same depth of analysis.
5. **Choose an action explicitly.** Review any quarantine, restore, destruction or cleanup selection before changing files. Scanning alone is not authorization to delete.
6. **Exit cleanly.** The desktop host is designed to stop its launched scanner/audit processes on cancellation or shutdown; final real-device acceptance remains part of the release gate.

### Integrated modules

| Module | Present capability | Important boundary |
| :--- | :--- | :--- |
| **Repository Security** | Go-based scanner for supported local inputs and public GitHub/Bitbucket source snapshots; 95 heuristic detection rules with 12 reported categories | Heuristics and inventory coverage do not prove complete malware detection |
| **Code & Secrets Audit** | In-repository Python W.H.I.T.E. replacement with selected credential/SAST heuristics, optional reachable local Git history and metadata-only/redacted JSON | **Offline CVE/SCA is unavailable**; complete legacy W.H.I.T.E. parity is unverified |
| **Windows Security** | System Sanitizer's local inspection, Defender integration, reviewed quarantine/restore and the original specialist GUI | Not a kernel antivirus replacement; existing antivirus software is not switched off |
| **Space Recovery** | Discover and select cleanup candidates, with confirmation before deletion | Cleanup is user-reviewed, not an automatic destructive scan |
| **Chrome Extension** | Manifest V3 popup to open supported repository targets in the installed local suite | Requires the desktop app and its registered `ots-security://` handler |

Windows Security Center integration can display registered antivirus protection status, including unavailable or inactive states, without installing or modifying antivirus products. See [AV-001 implementation evidence](docs/AV_001_ANTIVIRUS_COVERAGE.md).

### Architecture

```text
OTS Security Suite — native Windows WPF / WebView2 host
│
├── Repository Security
│   └── Go scanner · ephemeral 127.0.0.1 child service
│
├── Code & Secrets Audit
│   └── Python audit CLI · redacted JSON reports
│
├── Windows Security / Space Recovery
│   └── Vendored System Sanitizer .NET core
│       ├── Windows/Defender inspection
│       ├── User-scoped encrypted quarantine and restore
│       └── Explicitly confirmed cleanup
│
├── Optional original Sanitizer specialist window
└── Optional Chrome MV3 extension → ots-security://
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
| Windows security & cleanup | **Vendored System Sanitizer / .NET** | Existing Windows scanning, quarantine, restoration and cleanup implementation |
| Browser integration | <img src="https://skillicons.dev/icons?i=js,chrome" height="22" alt="JavaScript and Chrome" /> **Chrome Manifest V3 / JavaScript** | Optional local suite launcher |
| Packaging | **Inno Setup 6 / PyInstaller** | Windows installer and bundled Python executable |
| Engineering validation | <img src="https://skillicons.dev/icons?i=githubactions" height="22" alt="GitHub Actions" /> **GitHub Actions** | Recorded Windows integration builds and smoke checks |

**Licensing:** The repository does not currently provide a verified root distribution license. Component-specific license and third-party notices must be honored. Do not assume the complete suite is MIT-licensed merely because incorporated components carry their own licenses.

---

## <img src="art/readme-icons/installation.svg" width="24" height="24" alt="" /> Installation & Usage

### Supported development target

- **OS target:** Windows 10/11 x64; independent clean-device compatibility acceptance is still pending.
- **Build prerequisites:** .NET 10 SDK, Go 1.27.4 or newer compatible toolchain, Python 3.12, PyInstaller, Inno Setup 6 and Microsoft WebView2 Runtime.
- **Source:** The full System Sanitizer source is vendored in this repository, so a recursive Git submodule checkout is not required.
- **Distribution:** An older unsigned draft test installer exists for the owner. There is **no signed, publicly approved production release**.

### Get the source

```powershell
git clone https://github.com/ohanatechsolutions/OTS_Security_Suite.git
cd OTS_Security_Suite
```

### Build the Windows integration preview

From the repository on GitHub, use **Actions → Windows build and integration checks → Run workflow** where available and permitted. That workflow compiles the WPF host, Go engine, Python audit executable and bundled Sanitizer before producing an Inno Setup test installer.

For detailed environment, provenance, verification and packaging requirements, consult the [build blueprint](docs/BUILD_BLUEPRINT.md), [test evidence](docs/TEST_EVIDENCE.md) and [release gates](docs/RELEASE-GATES.md). Do not assume an older draft installer corresponds to the latest `main` commit.

### Run selected developer checks

```bash
(cd scanner && go test ./... && go vet ./...)
python -m unittest discover -s tools -v
node --check extension/popup.js
```

These are **component checks**, not a substitute for a Windows desktop build, clean install, GUI acceptance or security review.

### First use and safety

Choose the relevant module, select a target you own or are authorized to inspect, run an inspection and review the findings before requesting any changes. Remote repository scans require network access; supported local analysis does not need an external scanning service.

The suite does not execute downloaded repository code as part of routine scanning. Never treat a clean heuristic result as proof of safety. Existing antivirus protection should remain enabled. User-scoped quarantine is not equivalent to kernel-level malware isolation, and destructive actions require deliberate review and confirmation.

Unsigned development builds may produce Microsoft SmartScreen or antivirus reputation warnings; signing and reputation verification remain open release work.

---

## <img src="art/readme-icons/roadmap.svg" width="24" height="24" alt="" /> Roadmap & Verification

Checked items refer to **implemented functionality with the repository's recorded scoped engineering evidence**, not certification or complete release readiness.

- [x] **Native integration shell:** WPF desktop host and WebView2 scanner integration built and exercised in recorded Windows CI.
- [x] **Repository inspection:** Go scanner and 95-rule inventory covered by recorded automated tests.
- [x] **Local audit preview:** Redacted Python report workflow with explicit incomplete result states.
- [x] **Sanitizer source consolidation:** Original 42-file source snapshot vendored into the suite without a Git submodule; verified in [consolidation audit](docs/REPOSITORY_CONSOLIDATION_2026-11-18.md).
- [x] **Antivirus coexistence indicator:** Windows Security Center status handling covered by recorded [AV-001 CI](docs/AV_001_ANTIVIRUS_COVERAGE.md).
- [x] **Windows packaging in CI:** Unsigned Inno test installer and synthetic installed-app checks recorded in the engineering evidence.
- [ ] **Independent Windows acceptance:** Clean Windows 10/11 installation, actual DPI, taskbar/icon and accessibility checks.
- [ ] **Historical data validation:** Verify recovery of genuine legacy encrypted quarantine/vault data.
- [ ] **Browser acceptance:** Clean-profile Chrome extension testing and normal install/uninstall behavior.
- [ ] **W.H.I.T.E. capability parity:** Reconcile unrecovered historical detection behavior and missing offline dependency CVE/SCA.
- [ ] **Repository retirement clearance:** Independently archive the legacy Sanitizer Git history and security-held draft release before the owner considers deletion.
- [ ] **Public release gates:** Root license decision, distribution review, publisher signing, security acceptance and owner go/no-go approval.

**Repository retirement warning:** The former OTS W.H.I.T.E. repository is inaccessible; its original source history has not been recovered into this suite. The legacy System Sanitizer source has been incorporated, but its old repository is **not yet safe to delete**. Follow the [retirement runbook](docs/SANITIZER_REPO_RETIREMENT_2026-11-18.md) and preserve its historical refs and unpublished held assets separately.

**Current handoff:** [Project status](docs/PROJECT_STATUS.md) · [Test evidence](docs/TEST_EVIDENCE.md) · [Owner finish line](docs/OWNER_FINISH_LINE.md) · [Release gates](docs/RELEASE-GATES.md)

---

## <img src="art/readme-icons/acknowledgements.svg" width="24" height="24" alt="" /> Acknowledgements

This suite incorporates the Ohana Tech Solutions repository scanning codebase and the original OTS System Sanitizer source, with its own documentation, notices and component-specific licensing. The replacement W.H.I.T.E. auditor is new in-repository code, **not** a restored copy of the deleted historical codebase. Third-party components, including those packaged with the scanner and Windows tooling, retain their respective legal notices; review the repository's component notices and [source provenance](docs/REPOSITORY_CONSOLIDATION_2026-11-18.md) before redistribution.

---

<p align="center">
  <sub>Developed by <b>Ohana Tech Solutions</b>.</sub>
</p>
