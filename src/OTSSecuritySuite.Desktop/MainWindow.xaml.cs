using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using OTSSystemSanitizer.Core;

namespace OTSSecuritySuite.Desktop;

public partial class MainWindow : Window
{
    private Process? _engine;
    private CancellationTokenSource? _scanCancellation;
    private readonly QuarantineVaultRegistry _quarantine = new();
    private readonly HashSet<string> _reviewed = new(StringComparer.Ordinal);
    private readonly string _engineDirectory = Path.Combine(AppContext.BaseDirectory, "engines");
    private string? _launchRepository;
    private bool _isClosing;
    private bool _networkRefreshing;
    private AntivirusAssessment? _antivirusAssessment;

    public MainWindow()
    {
        InitializeComponent();
        // Startup and later browser protocol messages share one strict parser.
        foreach (var arg in Environment.GetCommandLineArgs().Skip(1))
            if (TrustedProtocol.TryParse(arg, out var repo) && repo is not null)
                _launchRepository = repo;
        RefreshVault();
    }

    private async void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        FitInitialWindowToWorkArea();
        UpdateAntivirusDisplay();
        await StartRepositoryEngineAsync();
    }

    public void ReceiveProtocol(string uri)
    {
        if (_isClosing || !TrustedProtocol.TryParse(uri, out var repo)) return;
        if (WindowState == WindowState.Minimized) WindowState = WindowState.Normal;
        Show();
        Activate();
        if (repo is null) return;
        _launchRepository = repo;
        SetPanel(RepositoryPanel);
        if (RepositoryBrowser.CoreWebView2 is not null &&
            RepositoryBrowser.Visibility == Visibility.Visible)
            _ = LaunchRequestedRepositoryAsync();
    }

    private async Task LaunchRequestedRepositoryAsync()
    {
        if (_isClosing || _launchRepository is not { Length: > 0 } repo ||
            RepositoryBrowser.CoreWebView2 is null) return;
        _launchRepository = null;
        // The destination is a validated public root URL; the Go scanner still
        // independently revalidates before accessing the network.
        var json = System.Text.Json.JsonSerializer.Serialize(repo);
        var script = "(()=>{const el=document.getElementById('repoUrl');" +
            "if(!el)return;document.querySelector('.navitem[data-tab=\"scan\"]')?.click();" +
            "document.querySelector('.scan-mode[data-mode=\"repo\"]')?.click();" +
            "el.value=" + json + ";document.getElementById('scanRepo')?.click();})();";
        try { await RepositoryBrowser.ExecuteScriptAsync(script); }
        catch (Exception ex) { if (!_isClosing) RepositoryLoading.Text = "Could not start requested repository scan: " + ex.Message; }
    }

    private void FitInitialWindowToWorkArea()
    {
        // WorkArea and WPF Window bounds are both in device-independent pixels.
        // Keep the requested 1300x800 default when space allows, but never force
        // a window larger than the available desktop on scaled/small displays.
        var work = SystemParameters.WorkArea;
        var availableWidth = Math.Max(1, work.Width - 24);
        var availableHeight = Math.Max(1, work.Height - 24);
        MinWidth = Math.Min(900, availableWidth);
        MinHeight = Math.Min(650, availableHeight);
        Width = Math.Min(1300, availableWidth);
        Height = Math.Min(800, availableHeight);
        Left = work.Left + (work.Width - Width) / 2;
        Top = work.Top + (work.Height - Height) / 2;
    }

    private async Task StartRepositoryEngineAsync()
    {
        string exe = Path.Combine(_engineDirectory, "OTSScannerEngine.exe");
        if (!File.Exists(exe))
        {
            RepositoryLoading.Text = "Repository engine is missing from this installation. Run the full suite installer.";
            return;
        }
        try
        {
            var psi = new ProcessStartInfo(exe) {
                UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = false,
                CreateNoWindow = true, WorkingDirectory = _engineDirectory
            };
            psi.ArgumentList.Add("--no-browser");
            psi.ArgumentList.Add("--ephemeral");
            psi.ArgumentList.Add("--print-url");
            _engine = Process.Start(psi) ?? throw new InvalidOperationException("Engine did not start.");
            // The first line contains the one-time local session URL; never print it in diagnostics.
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(25));
            var line = await _engine.StandardOutput.ReadLineAsync(timeout.Token);
            if (_isClosing) { StopEngine(); return; }
            if (line is null || !line.StartsWith("OTS_SCAN_URL=", StringComparison.Ordinal))
                throw new InvalidOperationException("Engine did not provide a local session URL.");
            var target = line["OTS_SCAN_URL=".Length..];
            if (!Uri.TryCreate(target, UriKind.Absolute, out var address) || !address.IsLoopback || address.Scheme != "http")
                throw new InvalidOperationException("Engine returned an invalid local address.");
            string profile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Ohana Tech Solutions", "Security Suite", "WebView2");
            Directory.CreateDirectory(profile);
            var env = await CoreWebView2Environment.CreateAsync(null, profile);
            if (_isClosing) { StopEngine(); return; }
            await RepositoryBrowser.EnsureCoreWebView2Async(env);
            if (_isClosing) { StopEngine(); return; }
            RepositoryBrowser.CoreWebView2.Settings.AreDevToolsEnabled = false;
            RepositoryBrowser.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            RepositoryBrowser.CoreWebView2.Settings.IsWebMessageEnabled = false;
            // Only the embedded local scanner is permitted to interact with the suite.
            // Block new browser windows, external pop-ups, and web permission prompts.
            RepositoryBrowser.CoreWebView2.NewWindowRequested += (_, popup) => {
                popup.Handled = true;
            };
            RepositoryBrowser.CoreWebView2.PermissionRequested += (_, permission) => {
                permission.State = CoreWebView2PermissionState.Deny;
            };
            RepositoryBrowser.CoreWebView2.NavigationStarting += (_, nav) => {
                if (!Uri.TryCreate(nav.Uri, UriKind.Absolute, out var dest) ||
                    dest.Scheme != "http" || dest.Host != "127.0.0.1" || dest.Port != address.Port)
                    nav.Cancel = true;
            };
            RepositoryBrowser.Visibility = Visibility.Visible;
            RepositoryLoading.Visibility = Visibility.Collapsed;
            RepositoryBrowser.CoreWebView2.NavigationCompleted += (_, e) => {
                if (e.IsSuccess && !_isClosing)
                    _ = LaunchRequestedRepositoryAsync();
            };
            RepositoryBrowser.CoreWebView2.Navigate(target);
        }
        catch (Exception ex)
        {
            RepositoryLoading.Text = "Could not launch repository scanner: " + ex.Message;
            StopEngine();
        }
    }
    private void StopEngine()
    {
        try { if (_engine is { HasExited: false }) _engine.Kill(entireProcessTree: true); }
        catch (InvalidOperationException) { }
        catch (System.ComponentModel.Win32Exception) { }
        try { _engine?.Dispose(); } catch { }
        _engine = null;
    }
    private void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        _isClosing = true;
        _scanCancellation?.Cancel();
        StopEngine();
    }
    private void SetPanel(Grid selected)
    {
        foreach (var p in new[] { RepositoryPanel, SystemPanel, NetworkPanel, CleanupPanel })
            p.Visibility = p == selected ? Visibility.Visible : Visibility.Collapsed;
    }
    private void ShowRepository(object sender, RoutedEventArgs e) => SetPanel(RepositoryPanel);
    private void ShowSystem(object sender, RoutedEventArgs e) { SetPanel(SystemPanel); UpdateAntivirusDisplay(); }
    private void ShowCleanup(object sender, RoutedEventArgs e) { RefreshVault(); SetPanel(CleanupPanel); }

    private async void ShowNetwork(object sender, RoutedEventArgs e)
    {
        SetPanel(NetworkPanel);
        await RefreshNetworkAsync();
    }

    private async void RefreshNetworkClick(object sender, RoutedEventArgs e)
    {
        await RefreshNetworkAsync();
    }

    private async Task RefreshNetworkAsync()
    {
        if (_isClosing || _networkRefreshing) return;
        _networkRefreshing = true;
        NetworkStatus.Text = "Checking Windows Firewall and local TCP connections…";
        try
        {
            var result = await Task.Run(NetworkProtectionAudit.Read);
            if (_isClosing) return;
            NetworkStatus.Text = NetworkProtectionAudit.OverallStatus(result.Profiles);
            NetworkProfiles.ItemsSource = result.Profiles.Select(p => p.Name + ": " + p.Detail).ToArray();
            NetworkConnections.ItemsSource = result.Connections;
            NetworkCount.Text = result.NetworkError ??
                $"Showing {result.Connections.Count:N0} of {result.TotalConnections:N0} observed TCP connections. " +
                "This is an inventory, not proof an attack was blocked. Nothing was changed.";
        }
        catch (Exception ex)
        {
            if (!_isClosing)
            {
                NetworkStatus.Text = "Network status could not be verified. Windows protection was not modified.";
                NetworkCount.Text = ex.GetType().Name;
                NetworkProfiles.ItemsSource = null;
                NetworkConnections.ItemsSource = null;
            }
        }
        finally { _networkRefreshing = false; }
    }

    private async void AnalyzeFirewallRulesClick(object sender, RoutedEventArgs e)
    {
        if (_isClosing || _networkRefreshing) return;
        _networkRefreshing = true;
        FirewallRuleSummary.Text = "Reading Windows Firewall rules for advisory overlap hints…";
        try
        {
            var summary = await Task.Run(NetworkProtectionAudit.AnalyzeRules);
            if (!_isClosing) FirewallRuleSummary.Text = summary;
        }
        catch (Exception ex)
        {
            if (!_isClosing)
                FirewallRuleSummary.Text = "Rule analysis unavailable: " + ex.GetType().Name +
                    ". Windows Firewall was not modified.";
        }
        finally { _networkRefreshing = false; }
    }

    private void OpenWindowsSecurityClick(object sender, RoutedEventArgs e)
    {
        try { Process.Start(new ProcessStartInfo("windowsdefender:") { UseShellExecute = true }); }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Windows Security could not be opened: " + ex.Message,
                "Windows Security", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }



    private async void RunQuick(object sender, RoutedEventArgs e) => await RunSystemScan(ScanMode.Quick);
    private async void RunDeep(object sender, RoutedEventArgs e) => await RunSystemScan(ScanMode.Deep);
    private void CancelSystemScan(object sender, RoutedEventArgs e)
    {
        if (_scanCancellation is not { IsCancellationRequested: false }) return;
        SystemStatus.Text = "Cancelling Windows inspection...";
        SystemCancel.IsEnabled = false;
        _scanCancellation.Cancel();
    }
    private async Task RunSystemScan(ScanMode mode)
    {
        if (!QuickButton.IsEnabled || !DeepButton.IsEnabled) return;
        using var cancellation = new CancellationTokenSource();
        _scanCancellation = cancellation;
        QuickButton.IsEnabled = DeepButton.IsEnabled = false;
        SystemCancel.IsEnabled = true;
        SystemFindings.ItemsSource = null; // Never reuse stale findings for a new scan.
        SystemStatus.Text = mode == ScanMode.Quick ? "Quick scan started..." : "Deep scan started...";
        var progress = new Progress<ScanProgress>(p => { if (!_isClosing && !cancellation.IsCancellationRequested) SystemStatus.Text =
            $"{p.Phase} · {p.Files:N0} files reviewed · {p.Skipped:N0} inaccessible"; });
        try
        {
            var result = await ThreatScanner.RunAsync(mode, progress, cancellation.Token);
            if (_isClosing) return;
            if (result.Cancelled || cancellation.IsCancellationRequested)
            {
                SystemStatus.Text = $"Scan cancelled · {result.Files:N0} files reviewed before cancellation.";
                return;
            }
            SystemFindings.ItemsSource = result.Findings;
            SystemStatus.Text = $"Scan finished · {result.Files:N0} files · {result.Findings.Count} findings · " +
                $"{result.Skipped} inaccessible · {result.Elapsed:g}";
        }
        catch (OperationCanceledException) { if (!_isClosing) SystemStatus.Text = "Scan cancelled."; }
        catch (Exception ex) { if (!_isClosing) SystemStatus.Text = "Scan failed: " + ex.Message; }
        finally
        {
            if (ReferenceEquals(_scanCancellation, cancellation)) _scanCancellation = null;
            if (!_isClosing)
            {
                QuickButton.IsEnabled = DeepButton.IsEnabled = true;
                SystemCancel.IsEnabled = false;
            }
        }
    }
    private void RefreshAntivirus(object sender, RoutedEventArgs e) => UpdateAntivirusDisplay();

    private void UpdateAntivirusDisplay()
    {
        if (_isClosing) return;
        // WSC COM queries are performed on WPF's STA; a failed query is UNKNOWN, never "safe".
        _antivirusAssessment = WindowsAntivirusInventory.Read();
        var assessment = _antivirusAssessment;
        AntivirusSummary.Text = assessment.Summary;
        AntivirusSummary.Foreground = assessment.Coverage switch {
            AntivirusCoverage.Protected => System.Windows.Media.Brushes.LightGreen,
            AntivirusCoverage.Unprotected => System.Windows.Media.Brushes.OrangeRed,
            AntivirusCoverage.NeedsAttention => System.Windows.Media.Brushes.Gold,
            _ => System.Windows.Media.Brushes.LightGray
        };
        AntivirusProducts.Text = assessment.Products.Count == 0
            ? "No registered antivirus products reported."
            : string.Join("   |   ", assessment.Products.Select(p =>
                p.Name + ": " + p.State + (p.State == AntivirusRuntimeState.On &&
                !p.SignaturesCurrent ? " (signatures outdated)" : "")));
        AntivirusGuidance.Text = assessment.Guidance;
        ProtectionHeader.Content = assessment.Coverage switch {
            AntivirusCoverage.Protected => "ANTIVIRUS ON · VIEW STATUS",
            AntivirusCoverage.NeedsAttention => "ANTIVIRUS NEEDS UPDATE · REVIEW",
            AntivirusCoverage.Unprotected => "UNPROTECTED · OPEN WINDOWS SECURITY",
            _ => "ANTIVIRUS UNKNOWN · CHECK STATUS"
        };
        ProtectionHeader.Foreground = assessment.Coverage switch {
            AntivirusCoverage.Protected => System.Windows.Media.Brushes.LightGreen,
            AntivirusCoverage.Unprotected => System.Windows.Media.Brushes.OrangeRed,
            AntivirusCoverage.NeedsAttention => System.Windows.Media.Brushes.Gold,
            _ => System.Windows.Media.Brushes.LightGray
        };
    }

    private void OpenProtectionFromHeader(object sender, RoutedEventArgs e)
    {
        if (_antivirusAssessment?.Coverage == AntivirusCoverage.Unprotected)
            OpenWindowsSecurity(sender, e);
        else
            ShowSystem(sender, e);
    }

    private void OpenWindowsSecurity(object sender, RoutedEventArgs e)
    {
        try
        {
            // Official built-in Windows Security URI; never download or silently toggle an AV.
            Process.Start(new ProcessStartInfo("windowsdefender:") { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            SystemStatus.Text = "Windows Security could not be opened: " + ex.Message +
                ". Open Start and search for Windows Security > Virus & threat protection.";
        }
    }

    private async void RunDefender(object sender, RoutedEventArgs e)
    {
        UpdateAntivirusDisplay();
        if (_antivirusAssessment?.ActiveDefender != true)
        {
            SystemStatus.Text = "Microsoft Defender is not reported active. No scan requested. Open Windows Security to check your installed antivirus.";
            return;
        }
        SystemStatus.Text = "Requesting Microsoft Defender Quick Scan...";
        try { SystemStatus.Text = await DefenderIntegration.RunScanAsync(ScanMode.Quick, CancellationToken.None); }
        catch (Exception ex) { SystemStatus.Text = "Defender request failed: " + ex.Message; }
    }
    private void OpenSanitizer(object sender, RoutedEventArgs e)
    {
        string binary = Path.Combine(_engineDirectory, "OTSSystemSanitizer.exe");
        if (!File.Exists(binary)) { SystemStatus.Text = "Full Sanitizer module is missing."; return; }
        try { Process.Start(new ProcessStartInfo(binary) { UseShellExecute = true }); }
        catch (Exception ex) { SystemStatus.Text = "Could not open System Sanitizer: " + ex.Message; }
    }
    private void InvestigateFinding(object sender, RoutedEventArgs e)
    {
        if (SystemFindings.SelectedItem is not ThreatFinding finding) return;
        MessageBox.Show(this, ThreatInvestigator.Explain(finding), "Finding investigation", MessageBoxButton.OK, MessageBoxImage.Information);
    }
    private void QuarantineFinding(object sender, RoutedEventArgs e)
    {
        if (SystemFindings.SelectedItem is not ThreatFinding finding) return;
        if (MessageBox.Show(this, "Move the selected user file into encrypted quarantine?\n\n"+finding.Path,
            "Confirm quarantine", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        try
        {
            _quarantine.Quarantine(finding.Path, ThreatInvestigator.Explain(finding));
            SystemStatus.Text = "File quarantined. Review it in Quarantine & Cleanup.";
            RefreshVault();
        }
        catch (Exception ex) { SystemStatus.Text = "Quarantine failed: " + ex.Message; }
    }
    private void RefreshQuarantine(object sender, RoutedEventArgs e) => RefreshVault();
    private void RefreshVault()
    {
        _reviewed.Clear(); // Never reuse approval after contents of the vault change.
        try
        {
            QuarantineItems.ItemsSource = _quarantine.List();
        }
        catch (IOException ex)
        {
            QuarantineItems.ItemsSource = null;
            CleanupStatus.Text = "Quarantine listing unavailable: " + ex.Message;
        }
        catch (UnauthorizedAccessException ex)
        {
            QuarantineItems.ItemsSource = null;
            CleanupStatus.Text = "Quarantine access denied: " + ex.Message;
        }
    }
    private void InvestigateQuarantined(object sender, RoutedEventArgs e)
    {
        if (QuarantineItems.SelectedItem is not QuarantinedFile item) return;
        MessageBox.Show(this, ThreatInvestigator.QuarantineReport(item), "Quarantine review",
            MessageBoxButton.OK, MessageBoxImage.Information);
        _reviewed.Add(QuarantineVaultRegistry.ReviewIdentity(item));
    }
    private void RestoreQuarantined(object sender, RoutedEventArgs e)
    {
        if (QuarantineItems.SelectedItem is not QuarantinedFile item) return;
        if (MessageBox.Show(this, "Restore this file to its original location?\n" + item.OriginalPath,
            "Confirm restore", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        try { _quarantine.Restore(item); RefreshVault(); CleanupStatus.Text = "File restored."; }
        catch (Exception ex) { CleanupStatus.Text = "Restore failed: " + ex.Message; }
    }
    private void DestroyQuarantined(object sender, RoutedEventArgs e)
    {
        if (QuarantineItems.SelectedItem is not QuarantinedFile item) return;
        if (!_reviewed.Contains(QuarantineVaultRegistry.ReviewIdentity(item))) { CleanupStatus.Text = "Review the selected quarantine item before destroying it."; return; }
        if (MessageBox.Show(this, "PERMANENTLY destroy this reviewed quarantine item? This cannot be undone.\n"+item.OriginalPath,
            "Permanent removal", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        try { _quarantine.Destroy(item); _reviewed.Remove(QuarantineVaultRegistry.ReviewIdentity(item)); RefreshVault(); CleanupStatus.Text = "Reviewed quarantine item removed."; }
        catch (Exception ex) { CleanupStatus.Text = "Removal failed: " + ex.Message; }
    }
    private void DiscoverCleanup(object sender, RoutedEventArgs e)
    {
        var candidates = CleanupService.Discover(BrowserCache.IsChecked == true);
        CleanupItems.ItemsSource = candidates;
        CleanupStatus.Text = $"{candidates.Count:N0} cleanup candidates. Select the specific files you want to remove.";
    }
    private void RemoveCleanup(object sender, RoutedEventArgs e)
    {
        var selected = CleanupItems.SelectedItems.Cast<CleanupCandidate>().ToArray();
        if (selected.Length == 0) { CleanupStatus.Text = "Select one or more cleanup files first."; return; }
        if (MessageBox.Show(this, $"Permanently remove {selected.Length:N0} selected old temporary files?",
            "Confirm space recovery", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        var result = CleanupService.RemoveSelected(selected);
        CleanupStatus.Text = $"Removed {result.Removed} files, {result.BytesRemoved:N0} bytes. Failed: {result.Failures.Count}.";
        DiscoverCleanup(sender,e);
    }
}
