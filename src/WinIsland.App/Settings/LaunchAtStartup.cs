// LaunchAtStartup.cs: makes Windows start WinIsland at sign-in, or stops that (PRD FR-20).
// Without package identity it uses the registry's Run list (StartupRegistration). With identity it
// uses WinIsland's Windows startup task (WindowsStartupTask), because Windows then shows WinIsland
// a private copy of the Run list, and turning the switch off left the real entry in place (bug
// found by smoke test ST-08, 10 Nov 2026). StartupTaskPlan (Core) decides what to change.

using System.Diagnostics;
using System.Runtime.InteropServices;

using WinIsland.Core.Startup;
using WinIsland.Platform.Startup;

namespace WinIsland.App.Settings;

internal static class LaunchAtStartup
{
    public const string MissingTaskText =
        "Start with Windows needs the instant notifications setup again: run Install-WinIslandIdentity.ps1 in WinIsland's packaging folder.";

    public const string RefusedText = "Windows refused to change starting WinIsland with Windows.";

    /// <summary>Applies the switch. Returns the text to show in Settings, or null when Windows follows it.</summary>
    public static async Task<string?> ApplyAsync(bool launchAtStartup)
    {
        if (!PackageIdentity.IsPresent())
        {
            return ApplyRunList(launchAtStartup);
        }

        try
        {
            return await ApplyStartupTaskAsync(launchAtStartup);
        }
        catch (Exception error) when (error is COMException or UnauthorizedAccessException)
        {
            Trace.TraceWarning("Changing the startup task failed: {0} (HRESULT 0x{1:X8})", error.GetType().Name, error.HResult);
            return RefusedText;
        }
    }

    private static string? ApplyRunList(bool launchAtStartup)
    {
        string? executable = Environment.ProcessPath;
        if (executable is null)
        {
            Trace.TraceWarning("Start with Windows not changed: the path of WinIsland.App.exe is unknown");
            return RefusedText;
        }

        return StartupRegistration.Apply(launchAtStartup, executable) ? null : RefusedText;
    }

    private static async Task<string?> ApplyStartupTaskAsync(bool launchAtStartup)
    {
        if (await WindowsStartupTask.GetStatusAsync() is not StartupTaskStatus status)
        {
            return MissingTaskText;
        }

        StartupTaskDecision decision = StartupTaskPlan.Decide(launchAtStartup, status);
        StartupTaskStatus? result = decision.Action switch
        {
            StartupTaskAction.Enable => await WindowsStartupTask.EnableAsync(),
            StartupTaskAction.Disable => await WindowsStartupTask.DisableAsync(),
            _ => status,
        };

        if (decision.Problem is not null)
        {
            return decision.Problem;
        }

        return result is StartupTaskStatus after ? StartupTaskPlan.ProblemAfter(launchAtStartup, after) : MissingTaskText;
    }
}
