// PermissionPromptBehavior.cs: what happens around "Allow notification access" (FR-12).
// Allow opens the Windows notification settings page, where access is switched on. The prompt
// hides on its own after 10 seconds, like the hotkey conflict banner, so it never sits in front
// of a game (a small change from the prototype, where it stayed until clicked).

using System.Diagnostics;
using System.Windows.Threading;

using WinIsland.Core.Island;

namespace WinIsland.App.Notifications;

internal sealed class PermissionPromptBehavior : IDisposable
{
    private static readonly TimeSpan ShowFor = TimeSpan.FromSeconds(10);
    private const string NotificationSettingsPage = "ms-settings:privacy-notifications";

    private readonly IslandStateMachine _island;
    private readonly DispatcherTimer _timer;

    public PermissionPromptBehavior(IslandStateMachine island, PermissionPromptView prompt)
    {
        ArgumentNullException.ThrowIfNull(island);
        ArgumentNullException.ThrowIfNull(prompt);
        _island = island;

        _timer = new DispatcherTimer { Interval = ShowFor };
        _timer.Tick += (_, _) =>
        {
            _timer.Stop();
            _island.DismissPermissionPrompt();
        };

        prompt.AllowClicked += (_, _) => OpenSettings();
        _island.Changed += (_, _) =>
        {
            if (_island.IsPermissionPromptShown && !_timer.IsEnabled)
            {
                _timer.Start();
            }
        };
    }

    public void Dispose()
    {
        _timer.Stop();
    }

    private void OpenSettings()
    {
        _island.DismissPermissionPrompt();
        try
        {
            Process.Start(new ProcessStartInfo(NotificationSettingsPage) { UseShellExecute = true });
        }
        catch (Exception error) when (error is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            Trace.TraceWarning("Opening the notification settings page failed: {0}", error.GetType().Name);
        }
    }
}
