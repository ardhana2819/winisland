// App.xaml.cs: starts WinIsland. Makes sure only one copy runs (FR-6b), then shows the island
// window and the tray icon (FR-6), and keeps the tray menu in step with the island's state.

using System.Windows;

using WinIsland.App.Clock;
using WinIsland.App.Island;
using WinIsland.Core.Hotkeys;
using WinIsland.Core.Island;
using WinIsland.Core.Tray;
using WinIsland.Platform.Tray;

namespace WinIsland.App;

public partial class App : Application
{
    private SingleInstance? _singleInstance;
    private IslandWindow? _islandWindow;
    private TrayIcon? _trayIcon;
    private PauseEndTimer? _pauseEndTimer;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _singleInstance = new SingleInstance();
        if (!_singleInstance.IsFirstInstance)
        {
            Shutdown();
            return;
        }

        IslandStateMachine island = new();
        _pauseEndTimer = new PauseEndTimer(island);
        _islandWindow = new IslandWindow(island);
        _islandWindow.Show();

        _trayIcon = new TrayIcon();
        _trayIcon.PauseOrResumeRequested += (_, _) => PauseOrResume(island);
        _trayIcon.HideOrShowRequested += (_, _) => island.ToggleHidden();
        _trayIcon.QuitRequested += (_, _) => Shutdown();
        island.Changed += (_, _) => UpdateTray(island);
        UpdateTray(island);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        _pauseEndTimer?.Dispose();
        _islandWindow?.Close();
        _singleInstance?.Dispose();
        base.OnExit(e);
    }

    private static void PauseOrResume(IslandStateMachine island)
    {
        if (island.IsPaused)
        {
            island.Resume();
        }
        else
        {
            island.Pause(DateTimeOffset.Now);
        }
    }

    private void UpdateTray(IslandStateMachine island)
    {
        _trayIcon?.Update(
            TrayMenuLabels.PauseOrResume(island.IsPaused),
            TrayMenuLabels.HideOrShow(island.IsHiddenByUser, HotkeySet.Defaults.HideShow),
            island.IsPaused);
    }
}
