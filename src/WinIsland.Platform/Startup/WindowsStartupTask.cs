// WindowsStartupTask.cs: turns WinIsland's Windows startup task on or off (PRD FR-20). This is how
// an app with package identity starts with Windows: the task is declared in the identity package
// (packaging/AppxManifest.template.xml, TaskId below), and Windows starts WinIsland at sign-in
// while it is on. It shows in Windows Settings > Apps > Startup, where the user can turn it off too.
// Only works with package identity; StartupRegistration.cs is used without it.

using System.Runtime.InteropServices;

using Windows.ApplicationModel;

using WinIsland.Core.Startup;
using WinIsland.Platform.Diagnostics;

namespace WinIsland.Platform.Startup;

public static class WindowsStartupTask
{
    // Must match the TaskId in packaging/AppxManifest.template.xml.
    private const string TaskId = "WinIslandStartup";

    /// <summary>The task's state, or null when Windows has no such task (an identity package registered from an older manifest).</summary>
    public static async Task<StartupTaskStatus?> GetStatusAsync()
    {
        StartupTask? task = await FindAsync();
        return task is null ? null : ToStatus(task.State);
    }

    /// <summary>Asks Windows to turn the task on. Returns the state afterwards, or null when there is no task.</summary>
    public static async Task<StartupTaskStatus?> EnableAsync()
    {
        StartupTask? task = await FindAsync();
        return task is null ? null : ToStatus(await task.RequestEnableAsync());
    }

    /// <summary>Turns the task off. Returns the state afterwards, or null when there is no task.</summary>
    public static async Task<StartupTaskStatus?> DisableAsync()
    {
        StartupTask? task = await FindAsync();
        if (task is null)
        {
            return null;
        }

        task.Disable();
        return ToStatus(task.State);
    }

    private static async Task<StartupTask?> FindAsync()
    {
        try
        {
            return await StartupTask.GetAsync(TaskId);
        }
        catch (Exception error) when (error is ArgumentException or COMException)
        {
            // The identity package was registered from an older manifest without the task.
            PlatformLog.Problem("The startup task is missing; run Install-WinIslandIdentity.ps1 again", error);
            return null;
        }
    }

    private static StartupTaskStatus ToStatus(StartupTaskState state) => state switch
    {
        StartupTaskState.Enabled => StartupTaskStatus.Enabled,
        StartupTaskState.DisabledByUser => StartupTaskStatus.DisabledByUser,
        StartupTaskState.DisabledByPolicy => StartupTaskStatus.DisabledByPolicy,
        StartupTaskState.EnabledByPolicy => StartupTaskStatus.EnabledByPolicy,
        _ => StartupTaskStatus.Disabled,
    };
}
