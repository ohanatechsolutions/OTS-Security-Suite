> **NEW TOOLING SCOPE — Strix stays optional:** The previous Strix WPF adapter and its scoped tests remain genuine evidence. The latest architecture retires the accompanying custom RepoScan engine. The Strix adapter may be reused in the external-tools-only hub after revalidation. Actual Docker containment and local LLM runtime are still BLOCKED. See [TOOLING_DECISIONS.md](TOOLING_DECISIONS.md).

# STRIX-001 — Optional local-only Strix integration (IN PROGRESS)

Date: November 18, 2026. Repository branch: feature/strix-local-opt-in-no-sanitizer.
Decision: add an optional self-hosted Strix capability with no required paid API or software subscription. Separate product-scope decision: retire System Sanitizer from the shipped OTS Security Suite, while preserving existing user data and archival source.

## Implemented on the feature branch

- A dedicated Strix WPF tab, separate from the normal non-executing RepoScan and read-only Windows Security/Network Defense interfaces.
- Explicitly selected local Strix Windows CLI executable and source directory; an explicitly entered model ID with the openai/ prefix and HTTP URL at a numeric loopback address; authorization checkbox and a separate Yes/No runtime approval.
- A source snapshot copied into a per-run directory. Limits: 3,000 files, 100 MiB total, 20 MiB per file. Reject filesystem reparse points, reject copying source into itself, skip .git, node_modules, strix_runs, common credential directories (.ssh/.aws/.azure/.venv), and common secret files (.env*, .pem, .key, .p12, .pfx, id_rsa/id_ed25519, credentials.json). This is defense in depth, NOT complete secret discovery. Original files are never mounted directly by the adapter.
- Separate no-shell process with non-interactive quick mode, 30 model turns, an internal --max-budget 1 accounting bound, and a 30-minute timeout. This is NOT authorization to spend $1. The configured locally hosted model has all supported monetary token-pricing values set to zero; no paid provider credentials are passed.
- A fresh child environment containing only minimal Windows variables and local-model settings. Both primary and deduplication endpoints are local; Strix telemetry disabled; remote Docker overrides not inherited.
- Stdout and stderr drained without writing possibly sensitive excerpts to the Suite UI. Cancellation attempts to terminate the CLI process tree. The temporary source copy is deleted in finally; any local Strix report is left for manual review.
- Build-project reference to the Sanitizer core and its UI, cleanup, and separate executable packaging removed. The unchanged historical source archive remains under modules/OTS_System_Sanitizer for preservation.

## Known limitations and strict security boundaries

1. Docker container network isolation is NOT established by restricting the model endpoint to loopback. Strix may execute code or initiate network requests. Use only owned, non-production target copies inside a dedicated unprivileged test VM with separately enforced egress controls.
2. The Strix CLI may create Docker-managed containers that outlive the child process. Cancellation is best effort and does not constitute a complete Docker cleanup guarantee.
3. Source snapshots can still include secrets embedded in ordinary code or files, despite exclusion of known credential-file patterns. Do not select production code containing real secrets. Local reports may also contain private data; inspect, securely store or remove them manually.
4. No Strix executable, Docker image, local LLM weights, paid API or remote hosted service is downloaded, installed or launched by installing OTS Security Suite. First explicitly authorized execution may allow upstream Strix/Docker to download sandbox images; do not attempt this without reviewing network access.
5. Windows CLI flag/version compatibility, model tool-calling quality, Docker isolation, network egress, real report parsing, and Windows acceptance have NOT been verified in an independent runtime test.
6. Returning exit code 0 does not prove a vulnerability-free target. Stopped and incomplete conditions must be checked in the native Strix reports.
7. The original Sanitizer repository and any prior user DPAPI/quarantine vaults are NOT deleted or migrated as part of this change. Previously installed versions may contain the old executable until users follow a separate, data-preserving clean-upgrade procedure.

## Verification gates

| ID | Requirement | Current evidence |
|---|---|---|
| STRIX-T01 | Reject remote or credential-bearing LLM endpoints, missing executable, unsafe source roots | Synthetic C# test PASS within 29/29 audit suite, CI #44463640720 |
| STRIX-T02 | Start non-interactively; do not forward paid credentials; force local deduplication and zero token prices | Synthetic C# test PASS within 29/29 audit suite, CI #44463640720 |
| STRIX-T03 | Preserve source, skip Git metadata, enforce size/reparse/overlap limits | Synthetic C# test PASS within 29/29 audit suite, CI #44463640720 |
| STRIX-T04 | Authorization and cancellation controls in WPF; no Sanitizer UI | Python static checks and installed-app WPF UI smoke PASS, CI #44463640720 |
| STRIX-T05 | Windows build and unsigned installer contain neither Strix binary nor Sanitizer | Windows CI build, package and installed-app smoke PASS #44463640720; artifact upload SKIPPED for PR |
| STRIX-T06 | Real local Strix/Docker/LLM test in a controlled, owner-authorized Windows VM | BLOCKED: real environment and approval |
| STRIX-T07 | Independent network-egress, container lifetime, source-data and report privacy review | BLOCKED: security acceptance |

## Verification commands on Windows (do not launch Strix)

    python -m unittest discover -s tools -p 'test_*.py'
    dotnet run --project tests/OTSSecuritySuite.AuditSmoke/OTSSecuritySuite.AuditSmoke.csproj -c Release
    dotnet run --project tests/OTSSecuritySuite.AntivirusSmoke/OTSSecuritySuite.AntivirusSmoke.csproj -c Release
    dotnet publish src/OTSSecuritySuite.Desktop/OTSSecuritySuite.Desktop.csproj -c Release -r win-x64 --self-contained true -o bundle

## Governance / N/A decisions

- Owner-confirmed REQ-STRIX-001: optional local-only autonomous testing with no required recurring software fee; REQ-SCOPE-002: exclude System Sanitizer from product while preserving history and vault data.
- Applicable: requirements traceability, security threat modeling, supply-chain/license review, WPF engineering, unit/abuse tests, Windows packaging, agent-tool control and release gating.
- N/A: installing a hosted Strix platform, spending money on model APIs, deploying an autonomous cloud agent, performing pentests against third-party networks, and publishing a signed production installer. These actions are outside this development change.
- Release gate: IN PROGRESS / NO-GO. Automated tests and independent Windows/Strix QA must pass before release.

Upstream: https://github.com/usestrix/strix (Apache-2.0; external user-managed installation).