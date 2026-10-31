// App.xaml.cs: starts WinIsland. Makes sure only one copy runs (FR-6b), then shows the island
// window and the tray icon (FR-6), connects Spotify (FR-7), and keeps the tray menu in step with
// the island's state.

using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

using WinIsland.App.Clock;
using WinIsland.App.Island;
using WinIsland.App.Music;
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
    private IslandTimeTimer? _islandTimeTimer;
    private MusicController? _music;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Draw with the CPU instead of the graphics card (PRD G4, Diva's "make it lighter").
        // Measured on Diva's PC, start-up plus two blur swaps: same CPU time (3.3 s vs 3.5 s), working
        // set 136 MB instead of 152 MB, private memory 60 MB instead of 108 MB, and 0 MB instead of
        // 33 MB of video memory, so the island takes no video memory away from games.
        RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

        _singleInstance = new SingleInstance();
        if (!_singleInstance.IsFirstInstance)
        {
            Shutdown();
            return;
        }

        IslandStateMachine island = new();
        _islandTimeTimer = new IslandTimeTimer(island);
        _music = new MusicController(island, Dispatcher);
        _islandWindow = new IslandWindow(island, _music);
        _islandWindow.Show();

        _trayIcon = new TrayIcon();
        _trayIcon.PauseOrResumeRequested += (_, _) => PauseOrResume(island);
        _trayIcon.HideOrShowRequested += (_, _) => island.ToggleHidden(DateTimeOffset.Now);
        _trayIcon.QuitRequested += (_, _) => Shutdown();
        island.Changed += (_, _) => UpdateTray(island);
        UpdateTray(island);

        // Connect to Spotify last, so the island and tray are already up. StartAsync never throws:
        // without a media session the island stays a clock (FR-10).
        await _music.StartAsync();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        _music?.Dispose();
        _islandTimeTimer?.Dispose();
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
