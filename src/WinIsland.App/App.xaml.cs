// App.xaml.cs: starts WinIsland. Makes sure only one copy runs (FR-6b), then shows the island
// window and the tray icon, and closes both when Quit is chosen in the tray.

using System.Windows;

using WinIsland.App.Island;
using WinIsland.Core.Island;
using WinIsland.Platform.Tray;

namespace WinIsland.App;

public partial class App : Application
{
    private SingleInstance? _singleInstance;
    private IslandWindow? _islandWindow;
    private TrayIcon? _trayIcon;

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
        _islandWindow = new IslandWindow(island);
        _islandWindow.Show();

        _trayIcon = new TrayIcon();
        _trayIcon.QuitRequested += (_, _) => Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        _islandWindow?.Close();
        _singleInstance?.Dispose();
        base.OnExit(e);
    }
}
