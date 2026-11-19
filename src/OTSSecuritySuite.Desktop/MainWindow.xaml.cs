using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;

namespace OTSSecuritySuite.Desktop;

public partial class MainWindow : Window
{
    private Process? _engine;
    private readonly StrixIntegration _strix = new();
    private CancellationTokenSource? _strixCancellation;
    private string? _strixReportsDirectory;
    private readonly string _engineDirectory = Path.Combine(AppContext.BaseDirectory, "engines");
    private string? _launchRepository;
    private bool _isClosing;
    private bool _networkRefreshing;
    private bool _threatActivityRefreshing;
    private AntivirusAssessment? _antivirusAssessment;

    public MainWindow()
    {
        InitializeComponent();
        // Startup and later browser protocol messages share one strict parser.
        foreach (var arg in Environment.GetCommandLineArgs().Skip(1))
            if (TrustedProtocol.TryParse(arg, out var repo) && repo is not null)
                _launchRepository = repo;
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
        _strixCancellation?.Cancel();
        _strix.Cancel();
        StopEngine();
    }
    private void SetPanel(Grid selected)
    {
        foreach (var p in new[] { RepositoryPanel, SystemPanel, NetworkPanel, StrixPanel })
            p.Visibility = p == selected ? Visibility.Visible : Visibility.Collapsed;
    }
    private void ShowRepository(object sender, RoutedEventArgs e) => SetPanel(RepositoryPanel);
    private void ShowSystem(object sender, RoutedEventArgs e) { SetPanel(SystemPanel); UpdateAntivirusDisplay(); }
    private void ShowStrix(object sender, RoutedEventArgs e) => SetPanel(StrixPanel);

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

    private async void RefreshThreatActivityClick(object sender, RoutedEventArgs e)
    {
        if (_isClosing || _threatActivityRefreshing) return;
        _threatActivityRefreshing = true;
        ThreatActivityStatus.Text = "Reading recent Windows security events…";
        try
        {
            var report = await Task.Run(WindowsThreatActivity.Read);
            if (_isClosing) return;
            ThreatActivityItems.ItemsSource = report.Entries;
            var blocked = report.Entries.Count(x => x.IsWindowsBlocked);
            var notice = report.Warnings.Count == 0 ? "" :
                " Some logs could not be read: " + string.Join(" ", report.Warnings);
            ThreatActivityStatus.Text =
                $"Showing {report.Entries.Count:N0} recent Windows records (up to 200 in the last 7 days). " +
                $"{blocked:N0} report Windows-owned blocks. These are not confirmed hacking attempts or OTS blocks." +
                notice;
        }
        catch (Exception ex)
        {
            if (!_isClosing)
            {
                ThreatActivityItems.ItemsSource = null;
                ThreatActivityStatus.Text = "Security event history could not be read (" +
                    ex.GetType().Name + "). No Windows security settings were changed.";
            }
        }
        finally { _threatActivityRefreshing = false; }
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

    private void ChooseStrixExecutable(object sender, RoutedEventArgs e)
    {
        var choose = new OpenFileDialog {
            Title = "Select a trusted, separately installed Strix CLI executable",
            Filter = "Windows executable (*.exe)|*.exe", CheckFileExists = true, Multiselect = false
        };
        if (choose.ShowDialog(this) == true) StrixExecutable.Text = choose.FileName;
    }

    private void ChooseStrixSource(object sender, RoutedEventArgs e)
    {
        var choose = new OpenFolderDialog {
            Title = "Select a local repository you own or are authorized to test",
            Multiselect = false
        };
        if (choose.ShowDialog(this) == true) StrixSource.Text = choose.FolderName;
    }

    private async void RunStrix(object sender, RoutedEventArgs e)
    {
        if (_isClosing || _strix.IsRunning) return;
        if (StrixAuthorization.IsChecked != true)
        {
            StrixStatus.Text = "Explicit target ownership/authorization confirmation is required.";
            return;
        }

        StrixRunOptions options;
        try
        {
            options = StrixSafety.Validate(StrixExecutable.Text, StrixSource.Text,
                StrixModel.Text, StrixEndpoint.Text);
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException)
        {
            StrixStatus.Text = "Strix configuration rejected: " + ex.Message;
            return;
        }

        if (MessageBox.Show(this,
            "Start an autonomous pentest of a DISPOSABLE COPY of the selected local source?\n\n" +
            "Strix uses Docker, may execute source code and can make network requests. " +
            "OTS cannot guarantee Docker network isolation. Use an unprivileged disposable test machine, " +
            "and ensure the source is under your explicit authorization.\n\n" +
            "Only a numeric loopback LLM endpoint is permitted, and no paid API key is passed.",
            "Confirm Strix local assessment", MessageBoxButton.YesNo, MessageBoxImage.Warning)
            != MessageBoxResult.Yes) return;

        using var cancel = new CancellationTokenSource();
        _strixCancellation = cancel;
        _strixReportsDirectory = null;
        StrixRunButton.IsEnabled = false;
        StrixCancelButton.IsEnabled = true;
        StrixReportsButton.IsEnabled = false;
        StrixStatus.Text = "Preparing a bounded disposable source copy and launching local Strix...";
        try
        {
            var result = await _strix.RunAsync(options, cancel.Token);
            if (_isClosing) return;
            _strixReportsDirectory = result.ReportsDirectory;
            StrixReportsButton.IsEnabled = Directory.Exists(result.ReportsDirectory);
            StrixStatus.Text = result.ExitCode switch
            {
                0 => "Strix finished without a validated finding in this run. This is NOT proof that the target is secure.",
                2 => "Strix reported at least one finding. Review the local run report before drawing conclusions.",
                _ => $"Strix ended with exit code {result.ExitCode}; the assessment is incomplete or failed."
            };
            if (StrixReportsButton.IsEnabled)
                StrixStatus.Text += " Reports are stored locally and may contain sensitive data.";
        }
        catch (OperationCanceledException)
        {
            if (!_isClosing) StrixStatus.Text = "Strix was cancelled. Inspect Docker for any residual containers.";
        }
        catch (Exception ex)
        {
            if (!_isClosing) StrixStatus.Text = "Strix could not complete: " + ex.GetType().Name +
                ". Confirm Docker, the installed CLI and your local model are working.";
        }
        finally
        {
            if (ReferenceEquals(_strixCancellation, cancel)) _strixCancellation = null;
            if (!_isClosing)
            {
                StrixRunButton.IsEnabled = true;
                StrixCancelButton.IsEnabled = false;
            }
        }
    }

    private void CancelStrix(object sender, RoutedEventArgs e)
    {
        StrixCancelButton.IsEnabled = false;
        StrixStatus.Text = "Stopping Strix. Docker cleanup may still be required.";
        _strixCancellation?.Cancel();
        _strix.Cancel();
    }

    private void OpenStrixReports(object sender, RoutedEventArgs e)
    {
        if (_strixReportsDirectory is not { } directory || !Directory.Exists(directory)) return;
        try { Process.Start(new ProcessStartInfo("explorer.exe") { UseShellExecute = false,
            ArgumentList = { directory } }); }
        catch (Exception ex)
        {
            StrixStatus.Text = "Could not open the local reports directory: " + ex.GetType().Name;
        }
    }
}
