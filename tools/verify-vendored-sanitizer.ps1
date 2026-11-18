# Verify that System Sanitizer is an exact, self-contained source snapshot.
# This does not replace compilation, security review, or runtime acceptance tests.
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
Push-Location -LiteralPath $repository
try {
    $expected = 'c56ef135da8b509c88d4175feba1e6055fd4b6f7'
    $tree = ([string](& git rev-parse 'HEAD:modules/OTS_System_Sanitizer')).Trim()
    if ($LASTEXITCODE -ne 0 -or $tree -ne $expected) {
        throw "Sanitizer source-tree mismatch: expected $expected, got $tree"
    }
    $tracked = @(& git ls-tree -r HEAD -- modules/OTS_System_Sanitizer)
    if ($LASTEXITCODE -ne 0 -or @($tracked | Where-Object { $_ -match ' blob ' }).Count -ne 42) {
        throw 'Expected 42 vendored Sanitizer source files.'
    }
    $gitlinks = @(& git ls-files --stage -- modules/OTS_System_Sanitizer | Where-Object { $_ -match '^160000 ' })
    if ($gitlinks.Count -gt 0) { throw 'Sanitizer still contains a Git submodule link.' }
    $submodules = @(& git ls-tree HEAD -- .gitmodules)
    if ($submodules.Count -gt 0) { throw 'Unexpected .gitmodules dependency remains.' }
    Write-Host "PASS: 42 vendored Sanitizer files; exact tree $tree; no Git submodule dependency."
}
finally { Pop-Location }
