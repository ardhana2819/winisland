// App.xaml.cs: starts WinIsland. Shows the island window and the tray icon, and closes both
// when Quit is chosen in the tray.

using System.Windows;

using WinIsland.App.Island;
using WinIsland.Platform.Tray;

namespace WinIsland.App;

public partial class App : Application
{
    private IslandWindow? _islandWindow;
    private TrayIcon? _trayIcon;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _islandWindow = new IslandWindow();
        _islandWindow.Show();

        _trayIcon = new TrayIcon();
        _trayIcon.QuitRequested += (_, _) => Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        _islandWindow?.Close();
        base.OnExit(e);
    }
}
