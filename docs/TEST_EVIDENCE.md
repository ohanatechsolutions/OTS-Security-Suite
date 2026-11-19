> **Evidence E-HUB-RELEASE-001 — 2026-11-19 00:22 HST:** Source `main@b80cc086f7d87fd3a6c36966d70320ffa5d680a3`, tag `v0.10.0-test.1`, [pre-release](https://github.com/ohanatechsolutions/OTS-Security-Suite/releases/tag/v0.10.0-test.1) **published** with real 47,071,315-byte unsigned `OTS-Security-Suite-Setup-x64.exe` and `SHA256SUMS.txt`; release asset SHA-256 `6c1ea64a0e0aa15676cacbd59b16284fb9ece2da135ceaf953fb83e1da026503`. Windows Actions [#44504685976](https://github.com/ohanatechsolutions/OTS-Security-Suite/actions/runs/44504685976) **SUCCESS** both `windows-build` and `publish-v0-10-test1`: native hub build/PE package validation; previous 0.9.x release downloaded and hash-verified before sandboxed CI installation; in-place 0.10.0 upgrade removes OTS scanner while preserving synthetic per-user and Sanitizer vault canaries; genuine vendor entry present in installed UI; uninstall retains canaries; GitHub build attestation verified (not publisher Authenticode); release job independently re-downloaded installer and verified its checksum and byte identity. [Universal Security Actions #44504686333](https://github.com/ohanatechsolutions/OTS-Security-Suite/actions/runs/44504686333) **SUCCESS** on same SHA (Semgrep CE, TruffleHog, OSV, Trivy and applicable tests; conditional checks reported N/A where appropriate). **NOT RUN / OPEN:** authentic vendor scan workflows for most 14 tools, independent Windows 10 and 11 owner UI/accessibility, real DPAPI vault restoration, Strix sandbox runtime, manual third-party license audit and Windows publisher signing. QA/test pre-release **GO**, production **NO-GO**. Older evidence below remains historical, not proof of completed 14-engine integration.

> **2026-11-18 HST — E-HUB-002 (main integration)** — [PR #18](https://github.com/ohanatechsolutions/OTS-Security-Suite/pull/18) merged at commit `08698b3502b9fa1cf4464d21e47a583240f3d415`. [Main Windows QA #44472295287](https://github.com/ohanatechsolutions/OTS-Security-Suite/actions/runs/44472295287) **SUCCESS**: hub-only build, installer smoke, installed-app WPF navigation, C# security smoke and GitHub provenance attestation independently verified ([attestation 69515150](https://github.com/ohanatechsolutions/OTS-Security-Suite/attestations/69515150)); the main-build binary digest `35f0f687fb338efec72e03cb22eb37eda0f5668321c18fc2e37ce125153f3b08` is a distinct nondeterministic rebuild and not the earlier PR artifact checksum. [Main universal security #44472295611](https://github.com/ohanatechsolutions/OTS-Security-Suite/actions/runs/44472295611) **SUCCESS**, all applicable checks including Semgrep/TruffleHog/Trivy/OSV/NET, conditional Python type-check N/A. **No `.exe` uploaded for that main push** under the test-only artifact policy; the existing reviewed PR artifact is separate. No production approval, signing, real vendor scanner execution, independent Windows 10/11 or 0.9.x upgrade tests.

> **2026-11-18 HST — E-HUB-001** — Scope: HUB-001–005. Tested head `5a5361ada5242cb522eb853a98b15b11550d8355`. [Windows run #44470904063](https://github.com/ohanatechsolutions/OTS-Security-Suite/actions/runs/44470904063) **SUCCESS**: actual upstream hub compiled on Windows, packaging lacks old scanner binaries, real installed-app UI finds KopiaUI instead of RepoScan, PE/integrity checks, per-user install, launch and uninstall. Artifact ID 14504407023, 47,073,694-byte `OTS-Security-Suite-Setup-x64.exe`, SHA-256 `0C5E5DA072EC2A64D601D99AD38E2C60247BF994E846383205E232EBF51EF34C` matches included `SHA256SUMS.txt`. [Universal Security run #44470904447](https://github.com/ohanatechsolutions/OTS-Security-Suite/actions/runs/44470904447) **SUCCESS**, with real source tests, Semgrep CE, OSV, TruffleHog, Trivy and .NET checks. This is **CI-only/unsigned owner preview**; prior published `v0.9.0-test.5` is obsolete. **NOT RUN:** actual execution of upstream scanning programs, independent clean-machine Win10/11 QA, legacy installed-app upgrade and genuine vault recovery, accessible/visual testing and code-signing/legal distribution review.

> **CURRENT SCOPE OF TEST EVIDENCE:** All PASS entries below are valid only for their cited historical commits and code. They do not prove ClamAV, YARA-X, osquery, Hayabusa, Kopia, HardeningKitty, Sysmon, PE-sieve, desktop TruffleHog/Semgrep/OSV/SkillSpector or real Strix Docker integration. Source-of-truth: [TOOLING_DECISIONS.md](TOOLING_DECISIONS.md).

> **2026-11-18 — STRIX-001 feature branch CI evidence:** [Windows build #44463640720](https://github.com/ohanatechsolutions/OTS-Security-Suite/actions/runs/44463640720) **SUCCESS** for `e18d87023bac07fc56f4bd0ac0c30401d4606ac8`: Go, Python, in-repository audit; AuditSmoke **29/29 PASS**, including 3 Strix policy/snapshot tests; Windows antivirus **16 assertions PASS**; WPF publish; Inno installer assembly; installed-app UI Automation verified Strix tab and removed cleanup tab; same-version repair/uninstall preserved synthetic legacy vault canary. Package artifact upload was **SKIPPED**, by PR policy; no new downloadable installer/release. Actual Strix/Docker/local-model execution, network isolation, independent owner acceptance and signed distribution **NOT RUN / NO-GO**. Earlier evidence below applies to the indicated historical commits, not this feature without a matching run.

## 2026-11-18 — CONSOL-001 source consolidation (verified branch CI)

- Source PR [#5](https://github.com/ohanatechsolutions/OTS_Security_Suite/pull/5) merged into `main@d190b03726c08cc0dfb90d62592ff0355a0f930a`.
- Reviewed source commit `8a2e27b6ab47bbd8f0115175cdfa78b056ef57b2`; [Windows CI run 44301998625](https://github.com/ohanatechsolutions/OTS_Security_Suite/actions/runs/44301998625) `windows-build` SUCCESS, release job SKIPPED.
- GitHub main tree identity independently verified: `modules/OTS_System_Sanitizer` mode `040000`, SHA `c56ef135da8b509c88d4175feba1e6055fd4b6f7`, 42 blobs; `.gitmodules` absent; zero Git submodule links.
- CI step evidence: clean checkout (submodules disabled); source identity script PASS; Go tests/vet and 95-rule check; Python tests and packaged WHITE smoke; metadata-only WHITE source audit; Sanitizer smoke; 22/22 suite audit, cleanup and quarantine-regression tests; native WPF build; legal notices/provenance manifest; Inno package creation; installed GUI UI Automation, repair/uninstall and synthetic vault preservation PASS.
- **Still NOT RUN:** independent real Windows 10/11/manual graphics and accessibility acceptance, original DPAPI vault restore, legacy WHITE parity/SCA, clean clone on owner's device and standalone Git-history/release-asset archive. **NO public release, no repository deletion.**

> **2026-11-15 — VERIFIED owner-only test prerelease:** [GitHub Windows Actions #43820859841](https://github.com/ohanatechsolutions/OTS_Security_Suite/actions/runs/43820859841) **SUCCESS** (both Windows build and draft prerelease jobs), source commit 953f1f3037cb4a2f8781c95ae8823370db5ef086. The owner-visible [DRAFT v0.9.0-test.2 prerelease](https://github.com/ohanatechsolutions/OTS_Security_Suite/releases/tag/untagged-8b649701cd0ffb13c62d) contains a **real unsigned** OTS-Security-Suite-Setup-x64.exe (102,419,492 bytes, genuine MZ PE) and SHA256SUMS.txt. The authenticated GitHub Release API independently confirms draft=true, prerelease=true, both assets and installer digest sha256:f672384e94c26c97664a368dfba98fafcb52c17b62fb0ead4a3ad11ae7841dd2. The Windows job verified Go, 95 rules, 8/8 new Python WHITE tests, 22/22 .NET tests, WPF UI automation, Inno installation, repair/uninstall and synthetic quarantine preservation. **Only a TEST PREVIEW is available; independent Windows10/11 acceptance, genuine legacy vault QA, Chrome clean-profile, old WHITE detection parity, offline SCA, code signing and public release approval remain NOT VERIFIED / NO-GO.** The older test.1 draft is superseded. Further draft release creation is manual-only, with a unique tag, to prevent unintended repeat publication or CI costs.

> **2026-11-15 test preview verification:** Windows CI [#43820040810](https://github.com/ohanatechsolutions/OTS_Security_Suite/actions/runs/43820040810) **windows-build SUCCESS** for new in-repo W.H.I.T.E. engine commit f4aa0235f39ada08c78b0950bd0295abfc73116c. Observed 8/8 new Python engine tests, 22/22 .NET audit tests, 13/13 Chrome contracts, 17/17 RepoScan browser tests, 95 Go rules, metadata-only source/history audit 125 files with two medium and zero high/critical findings, real packaged WPF UI Automation, Inno install/repair/uninstall, and synthetic quarantine preservation. Real unsigned installer SHA-256: 1B356F70007C3F6ACD87EBCD7CE8175B4381E4FC18895A77604D3042988F8824, size 102426238 bytes. GitHub draft test.1 prerelease assets were created and independently verified by authenticated release API, but **release job FAILED** solely because its final verification queried a draft by tag (404). Workflow corrected for test.2; **new test.2 CI NOT YET VERIFIED**. Independent real-machine QA and signing remain OPEN. This is not full deleted WHITE feature parity.

> **2026-11-15 owner decision / new build candidate:** The owner deleted the standalone OTS W.H.I.T.E. repository and directed the suite to proceed independently. The previous WHITE Git submodule is **not** part of the new preview candidate. A new in-repository Python heuristic auditor replaces its CLI contract, but **full legacy WHITE parity and offline dependency CVE analysis are NOT VERIFIED / NOT AVAILABLE**. The new candidate must pass its own Windows CI before an unsigned **draft** test prerelease may be created. Previous successful CI and installer hashes apply only to historical submodule builds. Main and original Sanitizer remain unchanged. See [WHITE replacement scope](../white_engine/NOTICE.md) and [test preview handoff](TEST_PRE_RELEASE.md).

# OTS Security Suite — Test Evidence (actual results only)

**Updated:** 2026-11-13 (Pacific/Honolulu)  
**Baseline main commit:** `d45de9592345e0b43acda4226004d72e2a181195`  
**Active branch:** `work/white-audit-lifecycle-and-blueprint-20261111`  
**Environment:** Chat Linux session has no Windows/.NET GUI; code was tested on GitHub Actions `windows-latest` runner. See exact source SHA and workflow logs below. GitHub connector has repository write access.

## E-031–E-039: scoped 2026-11-13 engineering verification

**Last verified engineering-code commit:** `ba7562b41e7ec1abda69406fc0b1a235993e3df7` — [Windows CI #43414634076](https://github.com/ohanatechsolutions/OTS_Security_Suite/actions/runs/43414634076) **SUCCESS**, Windows runner. Actual logs: Go tests/vet and scanner build PASS; Chrome popup contracts **13/13 PASS**; embedded RepoScan URL normalization **17/17 PASS**; offline W.H.I.T.E. source/history audit **172 worktree files scanned, zero high/critical findings reported by the configured audit** (not an independent security certification); packaged WHITE checks PASS; 22/22 Windows C# process/report/quarantine tests PASS; Sanitizer tests PASS; native WPF publish PASS; Inno Setup package PASS; install/WPF UI Automation/second-launch handoff/normal close/repair/uninstall and synthetic user-vault retention PASS; source/submodule/compiler provenance manifest PASS. **Unsigned CI test installer SHA-256: `826DD311F59CAD0E4BEB7A2F1353921CD5C790D90577D73B5C99C40FF4C830AA`**. Artifact upload intentionally skipped on PR, no public signed release. Independent real-device QA, historical DPAPI recovery, Chrome clean-profile, publisher signing and final owner approval remain **NOT RUN/BLOCKED**.

| ID | Requirement/gate | Actual recorded evidence | Status and limits |
|---|---|---|---|
| E-031 | REQ-002/003/014 | Go unit/vet and engine build on Windows; cancelled report, remote decompression budget and local HTTP security tests; corrupt/mismatched/oversized history rejected, bounded atomic save | **PASS CI**, real hostile corpus/third-party penetration test **NOT RUN** |
| E-032 | REQ-004 | 10 synthetic detection/benign lookalike fixtures and existing 95-rule/12-category, 12-file full-inventory and archive tests | **PASS Go tests**, measured sensitivity/specificity and real malware families **NOT RUN** |
| E-033 | REQ-003/012 | Chrome popup 13/13 contract and embedded RepoScan input canonicalization 17/17 tests | **PASS Node VM synthetic tests**, clean Chrome profile and real GUI Cancel click **NOT RUN** |
| E-034 | REQ-005/013 | Offline WHITE source/history scanner completed on 172 source files with no high/critical matches under configured rules | **PASS CI scoped audit**; not a full dependency CVE scan, secret certification or external pentest |
| E-035 | REQ-005/006/008/009/010/015/017 | 22/22 Windows C# subprocess, JSON, import, quarantine/DPAPI and synthetic cleanup tests | **PASS Windows CI**; real historical user profile/manual destructive denial tests **NOT RUN** |
| E-036 | REQ-001/005/011 | Real packaged WPF started and UI Automation ran synthetic WHITE report; same-user second launch forwarded rather than duplicating scanner window | **PASS Windows CI**; actual DPI/taskbar and accessibility human review **NOT RUN** |
| E-037 | REQ-011/013 | CI-only Inno PE installer; internal 64-char hash check; synthetic uninstall/repair, preserved quarantine canary, proper URI registration/removal; bundled extension/legal notices | **PASS Windows CI**; signed distribution and independent clean Win10/11 acceptance **NOT RUN** |
| E-038 | REQ-011 | Bundle `BUILD-MANIFEST.json` matched checkout SHA, both submodule gitlinks, supported Go version, SHA-256 fingerprint of Inno compiler and direct component list | **PASS Windows CI**; full transitive signed SBOM **NOT RUN** |
| E-039 | REQ-016 | Unsigned test installer SHA-256: `826DD311F59CAD0E4BEB7A2F1353921CD5C790D90577D73B5C99C40FF4C830AA`, run 43414634076; PR artifact intentionally SKIPPED | **PASS SHA only**; Authenticode signer and released asset **BLOCKED** |

**Owner-only tasks:** see [OWNER_FINISH_LINE.md](OWNER_FINISH_LINE.md) and the [manual QA matrix](QA_ACCEPTANCE.md). No archive/history finding, rule count or successful build is evidence of an approved public release.

## Recorded evidence

| ID | Requirement | Tool / method | Observed outcome | Status and scope |
|---|---|---|---|---|
| E-001 | M0 | GitHub connector read `main` repository metadata/tree/files | `main@d45de9592345...` has WPF native host, Go scanner, submodule links to WHITE and Sanitizer, Inno installer, extension, workflow | PASS — **repository inspection only** |
| E-002 | REQ-011 baseline | GitHub REST `GET /actions/runs?per_page=5` | Run [#7](https://github.com/ohanatechsolutions/OTS_Security_Suite/actions/runs/43132451936), commit `d45de959...`, `conclusion=success`, 2026-11-12 01:13:21 UTC | PASS — **previous main-branch configured Windows CI incl. build/installer/engine session smoke only**; not tested against feature branch |
| E-003 | M1 code present | GitHub connector updates and content guard checks | `MainWindow.xaml.cs` gained `CancelWhiteAudit`, linked deadline/cancel token, child kill on close, stale-report clear, 16 Mi-character stdout and 128 Ki-character stderr limits, JSON object verification, and no display of raw stderr; XAML has wired `WhiteCancel` button | PASS — **static source change**, NOT Windows compile or runtime |
| E-010 | REQ-006 static wiring | GitHub connector fetched branch XAML/C#; checked 22 XAML event-handler names against C# and 7 cancellation/privacy code guards | 22/22 handlers have code references; 7/7 guards true | PASS — **static contract inspection only** |
| E-011 | REQ-005 JSON format contract | Read pinned W.H.I.T.E. submodule commit `5328f276` CLI/reporting | `--format json` returns `json.dumps(report.as_dict())`, object-valued report; exit 0/1/2 based on clean/findings/incomplete | PASS — **source contract inspection only** |
| E-012 | REQ-006/005 current branch static review | Fetched exact current `MainWindow.xaml` and `MainWindow.xaml.cs` via GitHub connector; checked event names, cancellation, safe output guards and invalid exit behavior | 22/22 XAML event names resolved, 10/10 code contract assertions true; XAML blob `69c313f2`, C# blob `d987be3f` | PASS — **static source only**; compilation/integration NOT RUN |
| E-004 | Tool limitation | `timeout 9 git ls-remote https://github.com/ohanatechsolutions/OTS_Security_Suite.git HEAD` in Linux | `Could not resolve host: github.com` | BLOCKED — local clone unavailable; GitHub connector used instead |
| E-005 | REQ-006/017 | Windows integration: start long-running W.H.I.T.E., click Cancel, inspect child process tree, retry, close window | **NOT RUN** — Windows test host unavailable | BLOCKED |
| E-006 | REQ-011/016 | Current branch Windows build/install/uninstall/upgrade, PE header SHA-256, signing verification | **NOT RUN** — no authorized run on this branch / Windows host | BLOCKED |
| E-007 | REQ-003/004/014 | Go fixture corpus / rule category coverage / archive fuzzing | **NOT RUN for current branch** | NOT RUN |
| E-008 | REQ-005/007/008/009/010 | W.H.I.T.E. + Sanitizer cross-module VM end-to-end, encrypted quarantine restore | **NOT RUN** | NOT RUN |
| E-009 | REQ-001/002/012/013 | GUI DPI, WebView2, custom protocol/Chrome, privacy/security verification | **NOT RUN** | NOT RUN |

**Important:** Static source change E-003 alone is not verified; newer E-013..E-021 document Windows CI subprocess and installer checks. No interactive GUI acceptance for REQ-006 has been performed. Passing baseline CI E-002 cannot be carried forward as verification of later unbuilt commits. The Go count/heuristic labels, malware classifications, GUI icon, and signature have not been independently validated in this session.

## E-022 through E-030: verified automated checks, with separate manual gaps

**Current actual Windows CI evidence:** [run 43189027296](https://github.com/ohanatechsolutions/OTS_Security_Suite/actions/runs/43189027296) **SUCCESS** for docs head `de277e5fa3bb2d7e1bd8b2aac362a3d9e74ff916` containing executable source `a2f39b812e61116ecaa3f4813909b662d4149224`, with `Audit runner tests: 15/15 PASS`, packaged WHITE success/incomplete contract, 95 scanner rules and Go tests/vet, WPF synthetic navigation and real CLI execution, logo PNG sampled transparency (1563×1563), Inno installer, bundled MIT/license notices, extension files, synthetic vault preservation, URI registration, normal close and uninstall. Installer SHA-256 `8872BB5E2039C7B5F6AA13EA7AC52276CB22C80AA88B00FCDEE6271B5BC1C929` (CI-created, deliberately not uploaded on draft PR). **Automated gates passed; human Win10/Win11, clean Chrome extension, legacy real-user vault, independent security and publisher signing remain NOT RUN / blocked.**

The individual rows distinguish source/CI PASS from manual acceptance that remains NOT RUN. Relevant job ID: `131083365201`. The log includes 15/15 audit/report/vault regression cases, real WHITE WPF UI automation, 95 rules, logo transparency, license files, and synthetic quarantine preservation. The signed-release gate remains NO-GO.

| ID | Requirements | What was added | Verification state |
|---|---|---|---|
| E-022 | REQ-003/014 | Go ScanDirContext/ScanRemoteContext cancellation unit tests, new AbortController Cancel Scan code | **PASS Go test/vet**; manual frontend Cancel click NOT RUN |
| E-023 | REQ-002/013 | Auth/session-origin and cross-origin denial, cancelled scan not persisted, HTTP server limits | **PASS Go CI tests**; independent penetration check NOT RUN |
| E-024 | REQ-008 | Both historical and current encrypted DPAPI/AES vault registry + round-trip synthetic restore | **PASS 15/15 C# smoke suite**; real historical Windows profile NOT RUN |
| E-025 | REQ-009 | Destruction review token unique to exact vault path + immutable SHA256 | **PASS C# synthetic smoke case**; real destructive denial QA NOT RUN |
| E-026 | REQ-005/015 | Validate WHITE JSON schema, complete flag, pending checks, errors and exit status | **PASS C# regression cases**; output corruption fail-closed verified synthetically |
| E-027 | REQ-001 | Stable AppUserModelID static assertion, PNG pixel alpha sample (1563×1563), WPF install/start | **PASS Windows CI**; taskbar pin appearance/DPI user test NOT RUN |
| E-028 | REQ-011/012 | Bundled Chrome extension, original MIT/third-party notice files; synthetic vault canary retained after uninstall | **PASS installed Windows smoke**; Chrome clean-profile activation NOT RUN |
| E-029 | REQ-003/014 | Decompression budget explicitly ErrCoverage instead of silent TAR EOF; remote synthetic fixture | **PASS Go tests/vet**; broad fuzz corpus NOT RUN |
| E-030 | REQ-005/011 | Installed WPF UI Automation opened Code Audit, typed safe fixture path, ran packaged WHITE, validated JSON, navigated Security | **PASS Windows CI automation**; independent interactive human workflow NOT RUN |

**Current verified CI log:** [43189027296](https://github.com/ohanatechsolutions/OTS_Security_Suite/actions/runs/43189027296), head `de277e5fa3bb2d7e1bd8b2aac362a3d9e74ff916`, C# `15/15 PASS`, SHA-256 `8872BB5E2039C7B5F6AA13EA7AC52276CB22C80AA88B00FCDEE6271B5BC1C929`. The PR deliberately SKIPPED upload/public distribution. **Previous verified pass remains E-013–E-021** at older source `59243e6b15c6` and [run 43172812782](https://github.com/ohanatechsolutions/OTS_Security_Suite/actions/runs/43172812782); the later [run 43174959907](https://github.com/ohanatechsolutions/OTS_Security_Suite/actions/runs/43174959907) passed 10/10 subprocess/report tests for `2d752f765373`. The current candidate hash above supersedes all previous CI installer hashes; no public artifact is available from this draft PR.

## Latest candidate: verified Windows CI results (supersedes old NOT RUN entries below where scoped)

**Evidence source:** [GitHub Actions run 43172812782](https://github.com/ohanatechsolutions/OTS_Security_Suite/actions/runs/43172812782), `windows-latest`, commit `59243e6b15c61203bca39ef9cd95701f09cfa2e9`. Completed **SUCCESS** 2026-11-12 07:11 UTC (2026-11-11 21:11 HST). Workflow step-level success independently checked using GitHub Actions job ID `131029875670` and decoded job logs. The upload-artifact step was **SKIPPED INTENTIONALLY** for draft PR.

| Evidence ID | Requirements | Command / observed output | Scope / result |
|---|---|---|---|
| E-013 | REQ-003/004/014 | Workflow `go test ./...`, `go vet ./...`; scanner 95-rule check and new synthetic TAR size/URL tests | **PASS** Windows CI Go unit and vet; accuracy corpus / external pentest NOT RUN |
| E-014 | REQ-012/013 | `python -m unittest discover -s tools -p 'test_*.py'`, `node --check extension/popup.js` | **PASS** manifest least-privilege and extension syntax; clean Chrome install NOT RUN |
| E-015 | REQ-005 | Pinned WHITE `python -m unittest discover -s tests -p 'test_*.py'`; log reports `Ran 36 tests ... OK` | **PASS** packaged source module tests; external OSV cache scenario NOT RUN |
| E-016 | REQ-005/015 | `OTSWhiteAudit.exe` with synthetic one-file local repository; JSON complete exit=0, and non-Git `--history` JSON incomplete exit=2 | **PASS** executable invocation and expected incomplete contract |
| E-017 | REQ-006/013/017 | `dotnet run --project tests/OTSSecuritySuite.AuditSmoke/OTSSecuritySuite.AuditSmoke.csproj -c Release`; log `Audit runner tests: 8/8 PASS` | **PASS** real process cancellation, timeout, size bounds, stderr privacy, concurrency, unsafe config; GUI click NOT RUN |
| E-018 | REQ-007/011 | Sanitizer smoke and `dotnet publish` WPF and Sanitizer; workflow build steps success | **PASS** build and component smoke; Defender/DPAPI manual paths NOT RUN |
| E-019 | REQ-002/004/011 | Started Go ephemeral local process; authenticated `/api/info` reports 95 rules, valid 200 session | **PASS** local engine session, not adversarial origin test |
| E-020 | REQ-001/002/011/017 | Inno build & install; installed WPF `OTSSecuritySuite.exe` remained running, log `Installed WPF suite stayed alive after launch (PID 4304)`, then `Suite window close and immediate child-process cleanup verified` | **PASS** installed app launch, normal window close and immediate Go child exit on CI; no human visual/icon/Alt+Tab or W.H.I.T.E. close-mid-audit GUI acceptance |
| E-021 | REQ-011/016 | Installer `MZ` PE header check, `Get-FileHash -Algorithm SHA256`, per-user install/URI registration/uninstall cleanup; logged `Installer, bundled modules, icon, URI registration and uninstaller smoke checks passed` | **PASS** CI installer smoke; clean independent Win10/Win11 upgrade/repair and Authenticode signature NOT RUN |

**CI-generated installer SHA-256 (not released):**
```text
389DBCFE20AE5CB29EAA4F17B5360DEF0DD14841FA4DE7F767301289AFABF092  OTS-Security-Suite-Setup-x64.exe
```
The PR deliberately does not publish an artifact. An executable existing transiently on CI does not mean a public or user-accessible installer has been released.

**Real CI failure and fix:** Run [43171854132](https://github.com/ohanatechsolutions/OTS_Security_Suite/actions/runs/43171854132) failed because PowerShell propagated an **expected** WHITE exit status 2 after validation. Code change reset `$global:LASTEXITCODE = 0` **only after** checking JSON incomplete and exit=2; subsequent final candidate CI run succeeded. Do not erase this regression record.

**Still NOT RUN / release blockers:** independent Windows 10/11 fresh install, upgrade/downgrade and repair, signed-publisher provenance, real Chrome extension install, DPI/taskbar visuals, keyboard/screen reader, Defender denied/UAC, legacy encrypted quarantine/restore, cleanup/deletion fail-safe, actual user file false positives, privacy egress and external security review. These remain NO-GO irrespective of successful CI.

## Windows acceptance procedure for M1 / TEST-006 + TEST-017

1. Check out current review branch with recursive submodules; record exact commit SHA and `git status --short`.
2. On Windows 11 x64 (then Windows 10 x64), run `dotnet build src/OTSSecuritySuite.Desktop/OTSSecuritySuite.Desktop.csproj -c Release`; record errors and exit status. Run the existing workflow's build/test equivalents only under approved cost arrangements.
3. Run WPF suite; capture suite PID and Go process PID; start W.H.I.T.E. against a disposable synthetic local Git repository. Confirm "RUN" disabled and "CANCEL" enabled while running.
4. Click Cancel; verify UI says cancelled (not successful), W.H.I.T.E. PID and descendants exit within target (proposed ≤5 s), re-run works, previous report cannot be exported.
5. Force a long audit/timeout in a controlled test harness; verify process tree reaped after 12 minutes or a test-injected shorter deadline (never modify production timeout solely to make a CI test pass).
6. Close window mid-audit; verify neither W.H.I.T.E. nor its children remain; Go engine also exits. Test deny/failed startup and expected cleanup.
7. Include stdout/stderr under bounded synthetic flood and one stderr line that resembles sensitive data; ensure no unredacted data is displayed, exported, logged or uploaded.
8. Retain sanitized process/PID report and screenshots of Windows state; update this file with command, OS build, artifact hash and PASS/FAIL. A human verifies the complete user journey.

## Release blocking verification inventory

| Test ID | Target | Status |
|---|---|---|
| TEST-001 | WPF/taskbar titlebar logo and DPI sizing | WPF launch PASS CI; visual and DPI NOT RUN |
| TEST-002 | Go ephemeral session, WebView2 navigation and exit | CI ephemeral session/normal close PASS; adversarial UI security NOT RUN |
| TEST-003 | Repo/file/folder/archive E2E and abusive inputs | Go synthetic TAR/URL tests PASS; wider corpus NOT RUN |
| TEST-004 | 95-rule/12-category mapping + meaningful fixtures | 95-rule/12-category assertion PASS; real accuracy corpus NOT RUN |
| TEST-005 | W.H.I.T.E. real secrets/SAST/history/offline SCA | packaged benign+incomplete contract PASS; full scenarios NOT RUN |
| TEST-006 | Cancel/timeout/close and stale export | 8/8 subprocess smoke PASS; interactive GUI mid-audit NOT RUN |
| TEST-007 | Quick/Deep/Defender permissions | Sanitizer smoke PASS; manual permission paths NOT RUN |
| TEST-008 | DPAPI quarantine + restore across old versions | NOT RUN |
| TEST-009 | Investigate/review/destroy confirmation and denial | NOT RUN |
| TEST-010 | Cleanup select/partial failures/denial | NOT RUN |
| TEST-011 | Real Windows installer + upgrade/repair/uninstall | CI MZ/install/launch/close/URI/uninstall PASS; upgrade/repair/independent Win10/11 NOT RUN |
| TEST-012 | Chrome extension + custom protocol clean install | manifest/JS & Windows URI registration PASS; clean Chrome profile NOT RUN |
| TEST-013 | Local data/secret leak/egress controls | synthetic stderr privacy 8-test suite PASS; full egress NOT RUN |
| TEST-014 | Inventory, skipped and truncated verdict correctness | Go synthetic limit/TAR tests PASS; broad coverage acceptance NOT RUN |
| TEST-015 | Report export/invalid/empty/cancel states | packaged WHITE JSON output PASS; save/export UI NOT RUN |
| TEST-016 | Signing, PE, hash and release decision | PE + hash PASS; signature/release decision NOT RUN |
| TEST-017 | No surviving spawned scanner/auditor after close | CI WPF close-Go exit and WHITE subprocess tests PASS; UI close during WHITE audit NOT RUN |

**NO-GO:** There is no verified production release. No external security certification is claimed.
