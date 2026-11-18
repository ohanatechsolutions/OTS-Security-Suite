# Retire-preparation ONLY. Downloads/validates a private local archive.
# Never deletes, publishes, uploads, rewrites history, or executes an installer.
param(
    [Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [string] $Destination
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$source = 'ohanatechsolutions/OTS_System_Santizer'
$suite = 'ohanatechsolutions/OTS_Security_Suite'
$expectedHead = '823ad825dc1358009d3404f54604465c86747eba'
$expectedTree = 'c56ef135da8b509c88d4175feba1e6055fd4b6f7'
$heldReleaseId = 446563929
$heldReleaseTag = 'v0.4.0-legacy-security-hold'
$expectedSetupHash = '6aee66341f0cb5870be9832e04029f9fc5d524e63dc387d7c2bbc323c90bb358'

function Assert-Exit([string] $description) {
    if ($LASTEXITCODE -ne 0) { throw "$description failed (exit $LASTEXITCODE). No deletion is authorized." }
}
function Assert-Equal([string] $actual, [string] $expected, [string] $description) {
    if ($actual.Trim() -ne $expected) { throw "$description mismatch. Expected $expected; got $actual. Re-audit before retirement." }
}
function Get-Hash([string] $path) {
    return (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
}
foreach ($command in @('git','gh')) {
    if (-not (Get-Command $command -ErrorAction SilentlyContinue)) {
        throw "Required command '$command' is missing. Install Git for Windows and GitHub CLI."
    }
}
$full = [IO.Path]::GetFullPath($Destination)
if (Test-Path -LiteralPath $full) {
    throw "Destination already exists. Choose a NEW empty backup directory; never overwrite a prior archive: $full"
}
$parent = Split-Path -Parent $full
if (-not (Test-Path -LiteralPath $parent -PathType Container)) {
    throw "Parent backup directory must already exist: $parent"
}
# Auth is required to read the unpublished security-hold draft; no secrets are stored in the suite repo.
& gh auth status --hostname github.com *> $null
Assert-Exit 'GitHub CLI authentication (run gh auth login yourself if necessary)'
New-Item -ItemType Directory -Path $full -ErrorAction Stop | Out-Null
$mirror = Join-Path $full 'sanitizer-history.git'
$restored = Join-Path $full 'restore-test.git'
$bundle = Join-Path $full 'OTS_System_Santizer-all-refs.bundle'
$suiteCheckout = Join-Path $full 'suite-verification'
$metadata = Join-Path $full 'github-metadata'
$assets = Join-Path $full 'held-release-assets'
New-Item -ItemType Directory $metadata,$assets | Out-Null
Write-Host 'Downloading ALL reachable Sanitizer refs and Git history. Large download expected.'
& git clone --mirror "https://github.com/$source.git" $mirror
Assert-Exit 'Git mirror clone'
$head = [string](& git -C $mirror rev-parse refs/heads/main)
Assert-Exit 'Read archived main ref'
Assert-Equal $head $expectedHead 'Original Sanitizer main'
$tree = [string](& git -C $mirror rev-parse "$expectedHead^{tree}")
Assert-Exit 'Read archived source tree'
Assert-Equal $tree $expectedTree 'Original Sanitizer source tree'
& git -C $mirror fsck --full --no-reflogs
Assert-Exit 'Git mirror object integrity'
& git -C $mirror bundle create $bundle --all
Assert-Exit 'Create portable complete-ref Git bundle'
& git -C $mirror bundle verify $bundle
Assert-Exit 'Verify portable Git bundle'
# Restore from the portable bundle into a NEW mirror; do not rely on the source mirror itself.
& git clone --mirror $bundle $restored
Assert-Exit 'Git bundle restore test'
$restoreHead = [string](& git -C $restored rev-parse refs/heads/main)
Assert-Exit 'Read restored main'
Assert-Equal $restoreHead $expectedHead 'Restored main'
& git -C $restored fsck --full --no-reflogs
Assert-Exit 'Restored mirror integrity'
Write-Host 'Checking that Security Suite main contains exactly the original Sanitizer source tree.'
& git clone --depth 1 "https://github.com/$suite.git" $suiteCheckout
Assert-Exit 'Clone latest suite'
$includedTree = [string](& git -C $suiteCheckout rev-parse 'HEAD:modules/OTS_System_Sanitizer')
Assert-Exit 'Read vendored Sanitizer Git tree'
Assert-Equal $includedTree $expectedTree 'Suite embedded Sanitizer tree'
# GitHub-only records are NOT contained in Git bundles.
$releaseText = (& gh api "repos/$source/releases" --paginate --slurp) -join [Environment]::NewLine
Assert-Exit 'Fetch release inventory'
$releasePages = $releaseText | ConvertFrom-Json
$releases = @($releasePages | ForEach-Object { $_ })
# API --slurp returns an array of pages, each page containing release records.
$releases = @($releases | ForEach-Object { $_ })
$held = @($releases | Where-Object { $_.id -eq $heldReleaseId })
if ($held.Count -ne 1 -or -not $held[0].draft -or $held[0].tag_name -ne $heldReleaseTag) {
    throw 'Required unpublished security-hold release was not accessible. Abort retirement.'
}
if (@($held[0].assets).Count -ne 2) {
    throw 'Unexpected draft release asset inventory; re-audit before retirement.'
}
[IO.File]::WriteAllText((Join-Path $metadata 'releases.json'),$releaseText,[Text.UTF8Encoding]::new($false))
foreach ($record in @(
    @{ name = 'issues.json'; endpoint = "repos/$source/issues?state=all&per_page=100" },
    @{ name = 'pulls.json'; endpoint = "repos/$source/pulls?state=all&per_page=100" },
    @{ name = 'issue-comments.json'; endpoint = "repos/$source/issues/comments?per_page=100" },
    @{ name = 'workflow-runs.json'; endpoint = "repos/$source/actions/runs?per_page=100" }
)) {
    $json = (& gh api $record.endpoint --paginate --slurp) -join [Environment]::NewLine
    Assert-Exit "Export $($record.name)"
    $null = $json | ConvertFrom-Json
    [IO.File]::WriteAllText((Join-Path $metadata $record.name),$json,[Text.UTF8Encoding]::new($false))
}
# gh release download supports an explicit tag; authentication permits draft assets.
& gh release download $heldReleaseTag --repo $source --dir $assets --pattern '*'
Assert-Exit 'Download unpublished held-release assets'
$checksums = [ordered]@{}
foreach ($asset in @($held[0].assets)) {
    $path = Join-Path $assets ([string]$asset.name)
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing draft release asset $($asset.name)."
    }
    $item = Get-Item -LiteralPath $path
    if ($item.Length -ne [long]$asset.size) {
        throw "Release asset size mismatch: $($asset.name)"
    }
    $hash = Get-Hash $path
    if (-not ([string]$asset.digest).StartsWith('sha256:',[StringComparison]::OrdinalIgnoreCase)) {
        throw "Missing authoritative SHA-256 digest for $($asset.name)"
    }
    Assert-Equal $hash ([string]$asset.digest).Substring(7).ToLowerInvariant() "Release asset $($asset.name)"
    $checksums[$asset.name] = $hash
}
Assert-Equal $checksums['OTSSystemSanitizerSetup-x64.exe'] $expectedSetupHash 'Historical security-held installer'
$manifest = [ordered]@{
    archived_utc = [DateTime]::UtcNow.ToString('o')
    origin = $source
    original_main = $head.Trim()
    original_tree = $tree.Trim()
    suite_vendored_tree = $includedTree.Trim()
    refs_bundle_sha256 = Get-Hash $bundle
    refs_bundle_bytes = (Get-Item -LiteralPath $bundle).Length
    full_mirror_verified = $true
    portable_bundle_restore_verified = $true
    unpublished_held_release_id = $heldReleaseId
    unpublished_asset_sha256 = $checksums
    github_metadata = @('releases.json','issues.json','pulls.json','issue-comments.json','workflow-runs.json')
    independent_offsite_copy_verified = $false
    genuine_legacy_dpapi_restore_tested = $false
    historical_secret_review_complete = $false
    owner_retirement_signoff = $false
    repository_deletion_authorized = $false
}
$path = Join-Path $full 'RETIREMENT-ARCHIVE-MANIFEST.json'
$manifest | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $path -Encoding utf8
Write-Host 'Archive and bundle restore PASS. Draft installer hashes PASS. Suite source tree PASS.'
Write-Host "Private archive: $full"
Write-Warning 'NOT CLEARED TO DELETE. Copy to an encrypted OFFLINE destination, verify it there, resolve historical secret findings, test legacy DPAPI compatibility and record owner signoff.'
Write-Warning 'Held installer is security-blocked: DO NOT execute or publish it.'
