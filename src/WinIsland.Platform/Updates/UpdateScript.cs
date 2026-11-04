// UpdateScript.cs: starts packaging\Update-WinIsland.ps1 in its own window (PRD FR-46), for the
// Update button in Settings. WinIsland itself never goes online and never sees a token: the script
// does the checking and downloading, through the GitHub CLI already signed in on the PC.
// The script sits next to WinIsland in an installed or unzipped copy; a build run from the repo
// has none, so the button is greyed out there.

using System.ComponentModel;
using System.Diagnostics;

using WinIsland.Platform.Diagnostics;

namespace WinIsland.Platform.Updates;

public static class UpdateScript
{
    private const string ScriptName = "Update-WinIsland.ps1";

    /// <summary>The script next to WinIsland, or null when there is none.</summary>
    public static string? Find()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "packaging", ScriptName);
        return File.Exists(path) ? path : null;
    }

    /// <summary>Opens the script in a PowerShell window. Returns false when Windows refused.</summary>
    public static bool Start(string scriptPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scriptPath);
        try
        {
            // Shell start gives the script a window of its own; the path is quoted for its spaces.
            ProcessStartInfo start = new("powershell.exe", $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\"")
            {
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(scriptPath) ?? AppContext.BaseDirectory,
            };
            using Process? process = Process.Start(start);
            return process is not null;
        }
        catch (Exception error) when (error is Win32Exception or InvalidOperationException)
        {
            PlatformLog.Problem("Starting the update failed", error);
            return false;
        }
    }

    /// <summary>
    /// Runs the script's quiet check without a window (it reports only, nothing changes) and returns
    /// its output, or null when it fails or takes longer than the timeout. Started when the
    /// Settings window opens, so "Update available" can show (Diva, 4 Nov 2026).
    /// </summary>
    public static async Task<string?> CheckAsync(string scriptPath, TimeSpan timeout)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scriptPath);
        ProcessStartInfo start = new("powershell.exe", $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -File \"{scriptPath}\" -Quiet")
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            WorkingDirectory = Path.GetDirectoryName(scriptPath) ?? AppContext.BaseDirectory,
        };
        try
        {
            using Process process = Process.Start(start) ?? throw new InvalidOperationException("PowerShell did not start.");
            using CancellationTokenSource limit = new(timeout);
            try
            {
                string output = await process.StandardOutput.ReadToEndAsync(limit.Token);
                await process.WaitForExitAsync(limit.Token);
                return output;
            }
            catch (OperationCanceledException)
            {
                process.Kill(entireProcessTree: true);
                PlatformLog.Problem("Checking for an update took too long", new TimeoutException());
                return null;
            }
        }
        catch (Exception error) when (error is Win32Exception or InvalidOperationException)
        {
            PlatformLog.Problem("Checking for an update failed", error);
            return null;
        }
    }
}
