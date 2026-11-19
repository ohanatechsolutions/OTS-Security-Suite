using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace OTSSecuritySuite.Hub;

/// <summary>
/// A genuine launcher for separately installed upstream programs. It does not
/// impersonate their interfaces, install dependencies or run security operations.
/// </summary>
public partial class MainWindow : Window
{
    private readonly Dictionary<string, string> _connected = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _settingsPath;
    private ToolSpec? _selected;
    private Process? _helpProcess;
    private bool _closing;

    public MainWindow()
    {
        InitializeComponent();
        _settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Ohana Tech Solutions", "Security Suite", "hub-tool-paths.json");
        LoadConnections();
        ToolList.ItemsSource = ToolCatalog.All;
        ToolList.SelectedIndex = 0;
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        var area = SystemParameters.WorkArea;
        MinWidth = Math.Min(900, Math.Max(320, area.Width - 24));
        MinHeight = Math.Min(640, Math.Max(280, area.Height - 24));
        Width = Math.Min(1300, Math.Max(320, area.Width - 24));
        Height = Math.Min(800, Math.Max(280, area.Height - 24));
        Left = area.Left + (area.Width - Width) / 2;
        Top = area.Top + (area.Height - Height) / 2;
    }

    private void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        _closing = true;
        try
        {
            if (_helpProcess is { HasExited: false } process)
                process.Kill(entireProcessTree: true);
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception) { }
    }

    private void LoadConnections()
    {
        try
        {
            if (!File.Exists(_settingsPath)) return;
            var saved = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(_settingsPath));
            if (saved is null) return;
            foreach (var tool in ToolCatalog.All)
                if (saved.TryGetValue(tool.Id, out string? path) && ToolCatalog.ValidExecutable(tool, path))
                    _connected[tool.Id] = Path.GetFullPath(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            // Corrupt settings never grant arbitrary execution permission.
            _connected.Clear();
        }
    }

    private bool SaveConnections()
    {
        string? temp = null;
        try
        {
            string folder = Path.GetDirectoryName(_settingsPath)!;
            Directory.CreateDirectory(folder);
            temp = _settingsPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(_connected));
            File.Move(temp, _settingsPath, overwrite: true);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ActionStatus.Text = "Cannot save tool connection: " + ex.Message;
            return false;
        }
        finally
        {
            if (temp is not null)
                try { if (File.Exists(temp)) File.Delete(temp); }
                catch (IOException) { }
        }
    }

    private string? ResolveExecutable(ToolSpec tool)
    {
        if (_connected.TryGetValue(tool.Id, out string? selected) &&
            ToolCatalog.ValidExecutable(tool, selected))
            return selected;
        return ToolCatalog.FindOnPath(tool);
    }

    private void SelectTool(object sender, SelectionChangedEventArgs e)
    {
        if (ToolList.SelectedItem is not ToolSpec tool) return;
        _selected = tool;
        RenderTool();
    }

    private void RenderTool()
    {
        if (_selected is not { } tool) return;
        SelectedName.Text = tool.Name;
        SelectedCategory.Text = tool.Category;
        SelectedPurpose.Text = tool.Purpose;
        SelectedSafety.Text = tool.SafetyNote;
        HelpOutput.Visibility = Visibility.Collapsed;
        ActionStatus.Text = "";
        string? path = ResolveExecutable(tool);
        NativeIcon.Visibility = Visibility.Collapsed;
        FallbackMark.Visibility = Visibility.Visible;
        NativeIcon.Source = null;

        switch (tool.Mode)
        {
            case ToolMode.WindowsSecurity:
                ConnectionStatus.Text = "Built into Windows — open the actual Windows Security app";
                ExecutablePath.Text = "No extra antivirus is installed or substituted.";
                InterfaceNote.Text = "Windows Security opens in its original Windows interface. Existing antivirus products are left untouched.";
                break;
            case ToolMode.DocumentationOnly:
                ConnectionStatus.Text = "Upstream integration: manual setup required";
                ExecutablePath.Text = "Open the official project for its native installation and usage instructions.";
                InterfaceNote.Text = "No safe, standardized embeddable Windows GUI/launcher has been verified here. No fake vendor UI or dangerous automatic action is provided.";
                break;
            default:
                bool custom = _connected.ContainsKey(tool.Id) && path is not null;
                ConnectionStatus.Text = path is null ? "Not connected — locate an upstream installation" :
                    custom ? "Connected to your selected upstream executable" : "Found an upstream executable on PATH";
                ExecutablePath.Text = path ?? "Nothing was downloaded or installed by OTS Security Suite.";
                InterfaceNote.Text = tool.Mode == ToolMode.NativeGui
                    ? "OPEN launches the genuine installed vendor app, preserving its native interface and executable icon."
                    : "This upstream program is CLI-only. SHOW HELP runs only its --help command locally; scans and privileged actions are not started.";
                if (path is not null) ShowActualExecutableIcon(path);
                break;
        }
        OpenButton.IsEnabled = tool.Mode == ToolMode.WindowsSecurity ||
            (tool.Mode == ToolMode.NativeGui && path is not null);
        HelpButton.IsEnabled = tool.Mode == ToolMode.CommandLine && path is not null;
        LocateButton.IsEnabled = tool.Mode is ToolMode.NativeGui or ToolMode.CommandLine;
        UnlinkButton.IsEnabled = _connected.ContainsKey(tool.Id);
    }

    private void ShowActualExecutableIcon(string path)
    {
        try
        {
            using var icon = System.Drawing.Icon.ExtractAssociatedIcon(path);
            if (icon is null) return;
            var image = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            image.Freeze();
            NativeIcon.Source = image;
            NativeIcon.Visibility = Visibility.Visible;
            FallbackMark.Visibility = Visibility.Collapsed;
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or System.ComponentModel.Win32Exception) { }
    }

    private void ChooseExecutable(object sender, RoutedEventArgs e)
    {
        if (_selected is not { } tool || tool.Mode is not (ToolMode.NativeGui or ToolMode.CommandLine))
            return;
        var dialog = new OpenFileDialog
        {
            Title = "Connect the official installed executable for " + tool.Name,
            Filter = "Windows application (*.exe)|*.exe",
            CheckFileExists = true,
            Multiselect = false
        };
        if (dialog.ShowDialog(this) != true) return;
        if (!ToolCatalog.ValidExecutable(tool, dialog.FileName))
        {
            MessageBox.Show(this, "This file is not an expected executable for " + tool.Name +
                ". The hub does not execute arbitrary file names.", "Connection rejected",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        _connected[tool.Id] = Path.GetFullPath(dialog.FileName);
        bool saved = SaveConnections();
        RenderTool();
        ActionStatus.Text = saved ? "Connected. No software was installed or executed." :
            "Connected for this session; the path could not be saved.";
    }

    private void ForgetExecutable(object sender, RoutedEventArgs e)
    {
        if (_selected is not { } tool) return;
        _connected.Remove(tool.Id);
        bool saved = SaveConnections();
        RenderTool();
        ActionStatus.Text = saved ? "Tool location forgotten. The original application and its data were not changed." :
            "Connection cleared for this session; the saved setting could not be changed.";
    }

    private void RefreshTool(object sender, RoutedEventArgs e) => RenderTool();

    private void OpenOfficial(object sender, RoutedEventArgs e)
    {
        if (_selected is not { } tool ||
            !Uri.TryCreate(tool.OfficialUrl, UriKind.Absolute, out var url) ||
            url.Scheme != Uri.UriSchemeHttps)
            return;
        try
        {
            Process.Start(new ProcessStartInfo(url.AbsoluteUri) { UseShellExecute = true });
            ActionStatus.Text = "Opened the upstream project in your default browser.";
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            ActionStatus.Text = "Could not open the official website: " + ex.Message;
        }
    }

    private void OpenTool(object sender, RoutedEventArgs e)
    {
        if (_selected is not { } tool || _closing) return;
        try
        {
            if (tool.Mode == ToolMode.WindowsSecurity)
            {
                Process.Start(new ProcessStartInfo("windowsdefender:") { UseShellExecute = true });
                ActionStatus.Text = "Opened the genuine Windows Security interface.";
                return;
            }
            if (tool.Mode != ToolMode.NativeGui) return;
            string? exe = ResolveExecutable(tool);
            if (exe is null || !ToolCatalog.ValidExecutable(tool, exe))
            {
                ActionStatus.Text = "The connected upstream application is no longer available.";
                return;
            }
            Process.Start(new ProcessStartInfo(exe)
            {
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(exe)!
            });
            ActionStatus.Text = "Launched " + tool.Name + " in its original application window.";
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception or IOException)
        {
            ActionStatus.Text = "Could not launch upstream application: " + ex.Message;
        }
    }

    private static async Task<string> ReadBoundedAsync(StreamReader reader, int maxChars, CancellationToken cancellation)
    {
        var builder = new StringBuilder();
        char[] buffer = new char[2048];
        int read;
        while ((read = await reader.ReadAsync(buffer.AsMemory(), cancellation)) > 0)
            if (builder.Length < maxChars)
                builder.Append(buffer, 0, Math.Min(read, maxChars - builder.Length));
        return builder.ToString();
    }

    private async void ShowHelp(object sender, RoutedEventArgs e)
    {
        if (_selected is not { Mode: ToolMode.CommandLine } tool || _closing || _helpProcess is not null)
            return;
        string? exe = ResolveExecutable(tool);
        if (exe is null || !ToolCatalog.ValidExecutable(tool, exe))
        {
            ActionStatus.Text = "The upstream CLI executable is missing or no longer valid.";
            return;
        }
        HelpButton.IsEnabled = false;
        HelpOutput.Visibility = Visibility.Visible;
        HelpOutput.Text = "Reading the upstream tool's --help output locally...";
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo(exe)
            {
                UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardOutput = true, RedirectStandardError = true,
                WorkingDirectory = Path.GetDirectoryName(exe)!
            };
            process.StartInfo.ArgumentList.Add("--help");
            _helpProcess = process;
            if (!process.Start()) throw new IOException("Upstream process did not start.");
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var stdout = ReadBoundedAsync(process.StandardOutput, 12000, timeout.Token);
            var stderr = ReadBoundedAsync(process.StandardError, 4000, timeout.Token);
            try
            {
                await Task.WhenAll(stdout, stderr, process.WaitForExitAsync(timeout.Token));
                if (_closing) return;
                HelpOutput.Text = stdout.Result + (stderr.Result.Length > 0 ? "\n" + stderr.Result : "");
                ActionStatus.Text = $"Original {tool.Name} --help exited with status {process.ExitCode}. No security scan was run.";
            }
            catch (OperationCanceledException)
            {
                if (!process.HasExited) process.Kill(entireProcessTree: true);
                if (!_closing)
                {
                    HelpOutput.Text = "Timed out after 10 seconds; the help process was terminated.";
                    ActionStatus.Text = "The tool did not return help within the time limit.";
                }
            }
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception or IOException)
        {
            if (!_closing) ActionStatus.Text = "Cannot run original CLI help: " + ex.Message;
        }
        finally
        {
            _helpProcess = null;
            if (!_closing) RenderToolPreservingHelp();
        }
    }

    private void RenderToolPreservingHelp()
    {
        string output = HelpOutput.Text;
        string action = ActionStatus.Text;
        RenderTool();
        HelpOutput.Text = output;
        HelpOutput.Visibility = Visibility.Visible;
        ActionStatus.Text = action;
    }
}
