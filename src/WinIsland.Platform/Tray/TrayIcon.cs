// TrayIcon.cs: WinIsland's icon in the Windows notification area, with its menu.
// For now the menu only has Quit, so the app can always be closed. Pause for 1 hour,
// Hide island and Settings (PRD FR-6) are added in the tray pull request.

namespace WinIsland.Platform.Tray;

public sealed class TrayIcon : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _menu;
    private readonly Icon _icon;

    public TrayIcon()
    {
        _icon = TrayIconImage.Create();
        _menu = new ContextMenuStrip();
        _menu.Items.Add("Quit", image: null, (_, _) => QuitRequested?.Invoke(this, EventArgs.Empty));

        _notifyIcon = new NotifyIcon
        {
            Icon = _icon,
            Text = "WinIsland",
            ContextMenuStrip = _menu,
            Visible = true,
        };
    }

    public event EventHandler? QuitRequested;

    public void Dispose()
    {
        // Hide first, otherwise Windows can leave a ghost icon in the tray until the mouse passes over it.
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _menu.Dispose();
        _icon.Dispose();
    }
}
