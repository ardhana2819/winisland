// App.xaml.cs: starts WinIsland. Makes sure only one copy runs (FR-6b), turns on the error log
// and the crash guard, loads the saved settings (FR-19), shows the island window and the tray icon
// (FR-6), connects Spotify (FR-7) and the notification listener (FR-12), and keeps the tray menu in
// step with the island's state. Closing the island window closes the whole app (AC-9).

using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

using WinIsland.App.Clock;
using WinIsland.App.Diagnostics;
using WinIsland.App.Island;
using WinIsland.App.Music;
using WinIsland.App.Notifications;
using WinIsland.App.Settings;
using WinIsland.Core.Island;
using WinIsland.Core.Settings;
using WinIsland.Core.Tray;
using WinIsland.Platform.Settings;
using WinIsland.Platform.Tray;

namespace WinIsland.App;

public partial class App : Application
{
    private SingleInstance? _singleInstance;
    private IslandWindow? _islandWindow;
    private TrayIcon? _trayIcon;
    private IslandTimeTimer? _islandTimeTimer;
    private MusicController? _music;
    private NotificationController? _notifications;
    private SettingsController? _settings;
    private bool _isExiting;

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

        // Errors only, never message text (section 14), next to the settings file.
        Trace.Listeners.Add(new ErrorLogFile(SettingsFile.FolderPath));
        Trace.AutoFlush = true;
        CrashGuard.Install(Dispatcher);
        // The informational version keeps the "-beta.1" part; the plain assembly version cannot hold it.
        string version = typeof(App).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";
        Trace.TraceInformation("WinIsland {0} started", version);

        IslandStateMachine island = new();
        AppSettings saved = SettingsController.LoadSaved();
        _settings = new SettingsController(island, saved);
        _settings.ApplyAtStart();

        _islandTimeTimer = new IslandTimeTimer(island);
        _music = new MusicController(island, Dispatcher);
        _notifications = new NotificationController(island, Dispatcher);
        _islandWindow = new IslandWindow(island, _music, _notifications, saved.Hotkeys, saved.Size, saved.Look);
        _islandWindow.SettingsRequested += (_, _) => _settings.OpenWindow();
        _settings.Changed += (_, _) =>
        {
            _islandWindow.ApplySize(_settings.Current.Size);
            _islandWindow.ApplyLook(_settings.Current.Look);
        };
        // If Windows closes the island window (for example while signing out), close the whole app,
        // so nothing keeps running in the background without an island (AC-9).
        _islandWindow.Closed += (_, _) =>
        {
            if (!_isExiting)
            {
                Shutdown();
            }
        };
        _islandWindow.Show();
        if (_islandWindow.Hotkeys is IslandHotkeys hotkeys)
        {
            _settings.AttachHotkeys(hotkeys, _islandWindow.StartHotkeyConflict);
        }

        _trayIcon = new TrayIcon();
        _trayIcon.PauseOrResumeRequested += (_, _) => PauseOrResume(island);
        _trayIcon.HideOrShowRequested += (_, _) => island.ToggleHidden(DateTimeOffset.Now);
        _trayIcon.SettingsRequested += (_, _) => _settings.OpenWindow();
        _trayIcon.QuitRequested += (_, _) => Shutdown();
        island.Changed += (_, _) => UpdateTray(island);
        _settings.Changed += (_, _) => UpdateTray(island);
        UpdateTray(island);

        // Connect to Spotify and the notification listener last, so the island and tray are already
        // up. Side by side, because the music one keeps retrying when Windows refuses. Neither
        // throws: without them the island stays a clock (FR-10, section 13).
        await Task.WhenAll(_music.StartAsync(), _notifications.StartAsync());
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _isExiting = true;
        Trace.TraceInformation("WinIsland closing");
        _trayIcon?.Dispose();
        _music?.Dispose();
        _notifications?.Dispose();
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
        if (_settings is null)
        {
            return;
        }

        _trayIcon?.Update(
            TrayMenuLabels.PauseOrResume(island.IsPaused),
            TrayMenuLabels.HideOrShow(island.IsHiddenByUser, _settings.Current.Hotkeys.HideShow),
            island.IsPaused);
    }
}
