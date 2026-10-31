// TrayIcon.cs: WinIsland's icon in the Windows notification area, with its menu (PRD FR-6).
// Menu, in the design board's order: Pause for 1 hour (Resume while paused), Hide island
// (Show island while hidden), Settings, Quit.
// This class only shows the menu and reports clicks. The app decides what they do and tells
// the tray what to show through Update.

namespace WinIsland.Platform.Tray;

public sealed class TrayIcon : IDisposable
{
    // Design board: menu text 14, Segoe UI Variable.
    private static readonly Font MenuFont = new("Segoe UI Variable Text", 10.5f);

    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _menu;
    private readonly ToolStripMenuItem _pauseItem;
    private readonly ToolStripMenuItem _hideItem;
    private readonly Icon _normalIcon;
    private readonly Icon _dimmedIcon;

    public TrayIcon()
    {
        _normalIcon = TrayIconImage.Create(dimmed: false);
        _dimmedIcon = TrayIconImage.Create(dimmed: true);

        _pauseItem = CreateItem((_, _) => PauseOrResumeRequested?.Invoke(this, EventArgs.Empty));
        _hideItem = CreateItem((_, _) => HideOrShowRequested?.Invoke(this, EventArgs.Empty));
        ToolStripMenuItem settingsItem = CreateItem((_, _) => SettingsRequested?.Invoke(this, EventArgs.Empty));
        settingsItem.Text = "Settings";
        ToolStripMenuItem quitItem = CreateItem((_, _) => QuitRequested?.Invoke(this, EventArgs.Empty));
        quitItem.Text = "Quit";

        _menu = new ContextMenuStrip
        {
            Renderer = new DarkMenuRenderer(),
            BackColor = DarkMenuColors.Background,
            ForeColor = DarkMenuColors.Text,
            Font = MenuFont,
            ShowImageMargin = false,
        };
        _menu.Items.AddRange([_pauseItem, _hideItem, settingsItem, quitItem]);

        _notifyIcon = new NotifyIcon
        {
            Icon = _normalIcon,
            Text = "WinIsland",
            ContextMenuStrip = _menu,
            Visible = true,
        };
    }

    public event EventHandler? PauseOrResumeRequested;

    public event EventHandler? HideOrShowRequested;

    public event EventHandler? SettingsRequested;

    public event EventHandler? QuitRequested;

    /// <summary>Shows the menu wording and icon for the island's current state.</summary>
    public void Update(string pauseItemText, string hideItemText, bool isPaused)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pauseItemText);
        ArgumentException.ThrowIfNullOrWhiteSpace(hideItemText);

        _pauseItem.Text = pauseItemText;
        _hideItem.Text = hideItemText;
        _notifyIcon.Icon = isPaused ? _dimmedIcon : _normalIcon;
    }

    public void Dispose()
    {
        // Hide first, otherwise Windows can leave a ghost icon in the tray until the mouse passes over it.
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _menu.Dispose();
        _normalIcon.Dispose();
        _dimmedIcon.Dispose();
    }

    private static ToolStripMenuItem CreateItem(EventHandler onClick)
    {
        ToolStripMenuItem item = new() { ForeColor = DarkMenuColors.Text, Padding = new Padding(4, 6, 4, 6) };
        item.Click += onClick;
        return item;
    }
}
