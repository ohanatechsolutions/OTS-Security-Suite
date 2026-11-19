using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace OTSSecuritySuite.Desktop;

public sealed record StrixRunOptions(string Executable, string SourceDirectory, string Model, string LocalEndpoint);
public sealed record StrixRunResult(int ExitCode, string ReportsDirectory);

// Strix is a separately installed, opt-in tool. Never grant it the user's source checkout.
public static class StrixSafety
{
    public static StrixRunOptions Validate(string executable, string source, string model, string endpoint)
    {
        if (string.IsNullOrWhiteSpace(executable) || !Path.IsPathFullyQualified(executable))
            throw new ArgumentException("Choose an absolute path to a locally installed Strix executable.");
        string exe = Path.GetFullPath(executable);
        if (!string.Equals(Path.GetExtension(exe), ".exe", StringComparison.OrdinalIgnoreCase) || !File.Exists(exe))
            throw new ArgumentException("Strix must be an installed Windows .exe, selected explicitly.");
        if (string.IsNullOrWhiteSpace(source) || !Path.IsPathFullyQualified(source))
            throw new ArgumentException("Choose a local source-code folder.");
        string folder = Path.TrimEndingDirectorySeparator(Path.GetFullPath(source));
        if (!Directory.Exists(folder) || Path.GetPathRoot(folder) == folder || folder.StartsWith(@"\\", StringComparison.Ordinal))
            throw new ArgumentException("The source must be a local, non-root directory.");
        if ((File.GetAttributes(folder) & FileAttributes.ReparsePoint) != 0)
            throw new ArgumentException("Reparse points and directory links cannot be scanned.");
        model = model?.Trim() ?? "";
        if (!Regex.IsMatch(model, @"^[A-Za-z0-9][A-Za-z0-9._:/-]{1,127}$", RegexOptions.CultureInvariant))
            throw new ArgumentException("Enter a local model ID without spaces or control characters.");
        if (!Uri.TryCreate(endpoint?.Trim(), UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttp || !IPAddress.TryParse(uri.Host, out var ip) ||
            !IPAddress.IsLoopback(ip) || uri.Port <= 0 ||
            !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Query) ||
            !string.IsNullOrEmpty(uri.Fragment))
            throw new ArgumentException("Only an HTTP endpoint at numeric loopback (127.0.0.1 or ::1) is allowed.");
        return new StrixRunOptions(exe, folder, model, uri.AbsoluteUri);
    }

    public static ProcessStartInfo CreateStartInfo(StrixRunOptions config, string workDirectory, string snapshotDirectory, string configFile)
    {
        var info = new ProcessStartInfo(config.Executable) {
            UseShellExecute = false, CreateNoWindow = true, WorkingDirectory = workDirectory,
            RedirectStandardOutput = true, RedirectStandardError = true
        };
        foreach (var argument in new[] {
            "-n", "-t", snapshotDirectory, "--scan-mode", "quick",
            "--max-turns", "30", "--max-budget", "0",
            "--config", configFile
        }) info.ArgumentList.Add(argument);
        // No cloud providers, remote model endpoints, telemetry or inherited API credentials.
        foreach (string key in new[] {
            "OPENAI_API_KEY", "ANTHROPIC_API_KEY", "GOOGLE_API_KEY", "EXA_API_KEY",
            "PERPLEXITY_API_KEY", "STRIX_API_TOKEN", "OPENROUTER_API_KEY",
            "TRACELOOP_BASE_URL", "TRACELOOP_API_KEY", "STRIX_DEDUPE_API_KEY",
            "HTTP_PROXY", "HTTPS_PROXY", "ALL_PROXY"
        }) info.Environment.Remove(key);
        info.Environment["STRIX_LLM"] = config.Model;
        info.Environment["STRIX_DEDUPE_MODEL"] = config.Model;
        info.Environment["LLM_API_BASE"] = config.LocalEndpoint;
        info.Environment["LLM_API_KEY"] = "local-only";
        info.Environment["STRIX_API_TYPE"] = "chat_completions";
        info.Environment["STRIX_TELEMETRY"] = "0";
        info.Environment["NO_PROXY"] = "127.0.0.1,::1,localhost";
        return info;
    }

    public static void CopySnapshot(string source, string destination, CancellationToken cancellation)
    {
        const int maxFiles = 3000;
        const long maxBytes = 100L * 1024 * 1024;
        const long maxSingleFile = 20L * 1024 * 1024;
        int files = 0;
        long bytes = 0;
        string root = Path.GetFullPath(source);
        Directory.CreateDirectory(destination);
        var pending = new Stack<string>();
        pending.Push(root);
        while (pending.Count != 0)
        {
            cancellation.ThrowIfCancellationRequested();
            string directory = pending.Pop();
            foreach (var entry in new DirectoryInfo(directory).EnumerateFileSystemInfos())
            {
                cancellation.ThrowIfCancellationRequested();
                if ((entry.Attributes & FileAttributes.ReparsePoint) != 0)
                    throw new IOException("A link or reparse point was found. Refusing to copy this source.");
                if (entry is DirectoryInfo)
                {
                    if (entry.Name is ".git" or "node_modules" or "strix_runs") continue;
                    pending.Push(entry.FullName);
                    continue;
                }
                if (entry is not FileInfo file) continue;
                files++;
                bytes += file.Length;
                if (files > maxFiles || file.Length > maxSingleFile || bytes > maxBytes)
                    throw new IOException("Source snapshot exceeds the safe limit (3,000 files / 100 MiB total / 20 MiB each).");
                string relative = Path.GetRelativePath(root, file.FullName);
                string output = Path.GetFullPath(Path.Combine(destination, relative));
                if (!output.StartsWith(Path.GetFullPath(destination) + Path.DirectorySeparatorChar,
                        StringComparison.OrdinalIgnoreCase))
                    throw new IOException("A source path escaped the disposable workspace.");
                Directory.CreateDirectory(Path.GetDirectoryName(output)!);
                File.Copy(file.FullName, output, overwrite: false);
            }
        }
    }
}

public sealed class StrixIntegration
{
    private Process? _activeProcess;
    private int _busy;

    public bool IsRunning => Volatile.Read(ref _busy) != 0;
    public void Cancel()
    {
        try { if (_activeProcess is { HasExited: false } p) p.Kill(entireProcessTree: true); }
        catch (InvalidOperationException) { }
        catch (System.ComponentModel.Win32Exception) { }
    }

    public async Task<StrixRunResult> RunAsync(StrixRunOptions options, CancellationToken cancellation)
    {
        if (Interlocked.CompareExchange(ref _busy, 1, 0) != 0)
            throw new InvalidOperationException("A Strix assessment is already running.");
        string workRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Ohana Tech Solutions", "Security Suite", "StrixRuns",
            DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N"));
        string snapshot = Path.Combine(workRoot, "source-copy");
        try
        {
            // Validate again at execution boundary; no caller can bypass the policy.
            options = StrixSafety.Validate(options.Executable, options.SourceDirectory, options.Model, options.LocalEndpoint);
            Directory.CreateDirectory(workRoot);
            await Task.Run(() => StrixSafety.CopySnapshot(options.SourceDirectory, snapshot, cancellation), cancellation);
            // Override any account-wide Strix cloud configuration with a local-only file.
            string configPath = Path.Combine(workRoot, "local-config.json");
            var env = new Dictionary<string, string> {
                ["STRIX_LLM"] = options.Model, ["STRIX_DEDUPE_MODEL"] = options.Model,
                ["LLM_API_BASE"] = options.LocalEndpoint, ["LLM_API_KEY"] = "local-only",
                ["STRIX_API_TYPE"] = "chat_completions", ["STRIX_TELEMETRY"] = "0"
            };
            File.WriteAllText(configPath, JsonSerializer.Serialize(new { env }));
            using var proc = new Process { StartInfo = StrixSafety.CreateStartInfo(options, workRoot, snapshot, configPath) };
            _activeProcess = proc;
            if (!proc.Start()) throw new IOException("The locally installed Strix executable did not start.");
            // Drain both streams without storing potential source excerpts or secrets.
            var stdout = DrainAsync(proc.StandardOutput);
            var stderr = DrainAsync(proc.StandardError);
            using var bounded = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
            bounded.CancelAfter(TimeSpan.FromMinutes(30));
            try { await proc.WaitForExitAsync(bounded.Token); }
            catch (OperationCanceledException)
            {
                Cancel();
                await proc.WaitForExitAsync(CancellationToken.None);
                throw;
            }
            await Task.WhenAll(stdout, stderr);
            return new StrixRunResult(proc.ExitCode, Path.Combine(workRoot, "strix_runs"));
        }
        finally
        {
            _activeProcess = null;
            // Do not persist the user's copied source. Strix reports remain local and
            // may themselves contain sensitive data: the operator must review/delete them.
            try { if (Directory.Exists(snapshot)) Directory.Delete(snapshot, recursive: true); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            Interlocked.Exchange(ref _busy, 0);
        }
    }

    private static async Task DrainAsync(StreamReader reader)
    {
        char[] buffer = new char[4096];
        while (await reader.ReadAsync(buffer.AsMemory()) > 0) { }
    }
}
