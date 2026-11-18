# Synthetic, read-only Windows UI Automation check of the *installed* WPF host.
# No real user files are scanned or modified, no quarantine, Defender or cleanup.
param(
    [Parameter(Mandatory = $true)]
    [long] $WindowHandle
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

$root = [System.Windows.Automation.AutomationElement]::FromHandle([IntPtr]$WindowHandle)
if ($null -eq $root) { throw 'Cannot find installed suite main window via UI Automation' }
if ($root.Current.Name -notlike 'OTS Security Suite*') {
    throw "Unexpected installed suite window title: $($root.Current.Name)"
}
$rect = $root.Current.BoundingRectangle
if ($rect.Width -lt 700 -or $rect.Height -lt 600) {
    throw "Installed suite opened unusably small ($($rect.Width)x$($rect.Height))"
}

function Get-Ui([string] $automationId) {
    $condition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::AutomationIdProperty, $automationId)
    $item = $root.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
    if ($null -eq $item) { throw "Missing visible WPF UI control: $automationId" }
    return $item
}

function Click-Ui([string] $automationId) {
    $item = Get-Ui $automationId
    $pattern = $item.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
    $pattern.Invoke()
}

# A separate W.H.I.T.E. panel must not be present in the installed host.
$oldTab = [System.Windows.Automation.PropertyCondition]::new(
    [System.Windows.Automation.AutomationElement]::AutomationIdProperty, 'WhiteTab')
if ($root.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $oldTab)) {
    throw 'W.H.I.T.E. is still exposed as an end-user tab'
}
Click-Ui 'RepoTab'
Write-Host 'PASS: RepoScan is the only repository-scanning tab'
try {
    # A second native launch must hand off to the first and exit cleanly;
    # running a separate instance would create a second Go scanner.
    $exe = (Get-CimInstance Win32_Process -Filter "ProcessId=$($root.Current.ProcessId)").ExecutablePath
    if (-not $exe) { throw 'Cannot resolve installed suite executable for single-instance smoke' }
    $secondary = Start-Process -FilePath $exe -ArgumentList 'ots-security://scan' -PassThru
    try {
        if (-not $secondary.WaitForExit(15000)) { throw 'Secondary suite instance did not exit' }
        if ($secondary.ExitCode -ne 0) { throw "Secondary protocol activation failed with exit $($secondary.ExitCode)" }
        Write-Host 'PASS WPF UI Automation: second invocation handed off without second window'
    }
    finally {
        $secondary.Refresh()
        if (-not $secondary.HasExited) { Stop-Process -Id $secondary.Id -Force }
        $secondary.Dispose()
    }
    Click-Ui 'SystemTab'
    $system = Get-Ui 'SystemStatus'
    if ($system.Current.Name -notlike '*read-only*') {
        throw 'Windows Security tab did not show initial read-only confirmation'
    }
    Write-Host 'PASS WPF UI Automation: Windows Security tab navigation'
} finally {
    # No synthetic files or destructive operations were needed.
}
