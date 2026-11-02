// IslandWindow.xaml.cs: connects the island window to the island's rules.
// Makes the window non-activating (FR-3), places and scales it on the primary monitor (FR-1) at
// the size chosen in Settings (FR-32),
// registers the hotkeys (FR-5), shows the conflict banner for 10 seconds when a hotkey is taken,
// shows the clock (AC-1), shows music and opens or closes on click (FR-2, FR-7), shows
// notification banners (FR-15), switches the expanded island's pages by scroll, Alt+Up and
// Alt+Down or the page dots (FR-21), shows the unread dot on the compact island (FR-33), and
// animates whenever the island's state changes (FR-4).

using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

using Microsoft.Win32;

using WinIsland.App.Clock;
using WinIsland.App.Music;
using WinIsland.App.Notifications;
using WinIsland.Core.Hotkeys;
using WinIsland.Core.Island;
using WinIsland.Core.Music;
using WinIsland.Platform.Hotkeys;
using WinIsland.Platform.Windows;

namespace WinIsland.App.Island;

public partial class IslandWindow : Window
{
    private static readonly TimeSpan ConflictBannerDuration = TimeSpan.FromSeconds(10);

    private readonly IslandStateMachine _island;
    private readonly MinuteTimer _minuteTimer = new();
    private readonly DispatcherTimer _conflictTimer;
    private readonly PillAnimator _animator;
    private readonly MusicController _music;
    private readonly IslandMusicPresenter _musicPresenter;
    private readonly PointerLeaveCollapser _leaveCollapser;
    private readonly OutsideClickWatcher _outsideClickWatcher;
    private readonly IslandNotificationPresenter _notificationPresenter;
    private readonly PermissionPromptBehavior _permissionPrompt;
    private readonly HotkeySet _startHotkeys;
    private readonly PageScroll _pageScroll = new();
    private readonly PillColors _pillColors;
    private readonly IslandResizeAnimator _resizeAnimator;
    private ExpandedKeys? _expandedKeys;
    private TopmostGuard? _topmostGuard;
    private IslandView _shownView = IslandView.Compact;
    private PillShape _shownShape = PillShape.For(IslandView.Compact);
    private bool _shownVisible = true;
    private IslandSize _size;

    internal IslandWindow(IslandStateMachine island, MusicController music, NotificationController notifications, HotkeySet startHotkeys, IslandSize size)
    {
        ArgumentNullException.ThrowIfNull(island);
        ArgumentNullException.ThrowIfNull(music);
        ArgumentNullException.ThrowIfNull(notifications);
        ArgumentNullException.ThrowIfNull(startHotkeys);
        _island = island;
        _music = music;
        _startHotkeys = startHotkeys;
        _size = size;

        InitializeComponent();
        _animator = new PillAnimator(Pill, PillScale, PillOffset);
        _animator.SetShape(PillShape.For(IslandView.Compact));
        _musicPresenter = new IslandMusicPresenter(island, music, MusicLayer, ExpandedLayer.Music);
        _leaveCollapser = new PointerLeaveCollapser(island, Pill);
        _outsideClickWatcher = new OutsideClickWatcher(island, Pill);
        _notificationPresenter = new IslandNotificationPresenter(island, notifications, BannerLayer, ExpandedLayer.Notifications);
        ExpandedLayer.PageRequested += (_, page) => _island.ShowPage(page);
        _pillColors = new PillColors(Pill, PillShade);
        _resizeAnimator = new IslandResizeAnimator(IslandGrowScale);
        _music.ArtworkChanged += (_, _) => UpdatePillColors();
        _permissionPrompt = new PermissionPromptBehavior(island, PermissionLayer);

        _conflictTimer = new DispatcherTimer { Interval = ConflictBannerDuration };
        _conflictTimer.Tick += (_, _) =>
        {
            _conflictTimer.Stop();
            _island.DismissHotkeyConflict();
        };

        SourceInitialized += OnSourceInitialized;
        Closed += OnClosed;
    }

    protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
    {
        base.OnDpiChanged(oldDpi, newDpi);
        PlaceOnPrimaryDisplay();
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        IntPtr handle = new WindowInteropHelper(this).Handle;
        NonActivatingWindow.Apply(handle);
        _topmostGuard = new TopmostGuard(handle);
        HwndSource.FromHwnd(handle).AddHook(OnWindowMessage);

        PlaceOnPrimaryDisplay();
        SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;

        _minuteTimer.MinuteChanged += (_, now) =>
        {
            ClockLabel.Text = ClockText.Format(now);
            // Backup for the pause timer: also ends a pause that ran out while the PC was asleep.
            _island.UpdateTime(DateTimeOffset.Now);
        };
        _minuteTimer.Start();

        _island.Changed += (_, _) => ShowCurrentState();
        RegisterHotkeys(handle);
    }

    /// <summary>Raised when the conflict banner's Change button is clicked (open Settings).</summary>
    public event EventHandler? SettingsRequested;

    /// <summary>The island's hotkeys; Settings changes them. Available once the window is shown.</summary>
    internal IslandHotkeys? Hotkeys { get; private set; }

    /// <summary>
    /// A new island size from Settings (FR-32): shown at once, without a restart, growing or
    /// shrinking with the shape spring. Only the island inside changes; the window keeps its size
    /// and place, so the island stays centered the whole time (Diva, 2 Nov 2026).
    /// </summary>
    internal void ApplySize(IslandSize size)
    {
        if (size == _size)
        {
            return;
        }

        _resizeAnimator.FinishNow();
        double fromScale = LayoutFor(_size).Scale;
        _size = size;
        _resizeAnimator.Run(fromScale, LayoutFor(size).Scale, ApplyIslandScale);
    }

    /// <summary>The conflict found when the hotkeys were first registered, for the Settings window.</summary>
    internal string? StartHotkeyConflict { get; private set; }

    private void RegisterHotkeys(IntPtr handle)
    {
        Hotkeys = new IslandHotkeys(handle);
        Hotkeys.Global.Pressed += OnHotkeyPressed;
        _expandedKeys = new ExpandedKeys(_island, Hotkeys.Global);

        // At start a taken hotkey shows the conflict banner for 10 seconds (FR-5); Settings shows it too.
        string? conflictMessage = Hotkeys.Apply(_startHotkeys);
        StartHotkeyConflict = conflictMessage;
        if (conflictMessage is not null)
        {
            _island.ShowHotkeyConflict(conflictMessage);
            _conflictTimer.Start();
        }
    }

    private void OnConflictChangeClicked(object sender, RoutedEventArgs e)
    {
        _conflictTimer.Stop();
        _island.DismissHotkeyConflict();
        SettingsRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnHotkeyPressed(object? sender, HotkeyAction action)
    {
        switch (action)
        {
            case HotkeyAction.Expand:
                _island.ToggleExpanded(DateTimeOffset.Now);
                break;
            case HotkeyAction.HideShow:
                _island.ToggleHidden(DateTimeOffset.Now);
                break;
            case HotkeyAction.HideBanner:
                _island.HideNotifications(DateTimeOffset.Now);
                break;
            case HotkeyAction.MusicPlayPause:
                _ = _music.TogglePlayPauseAsync();
                break;
            case HotkeyAction.MusicNext:
                _ = _music.NextAsync();
                break;
            case HotkeyAction.MusicPrevious:
                _ = _music.PreviousAsync();
                break;
            case HotkeyAction.ShowNotificationsPage:
                _island.ShowPage(ExpandedPage.Notifications);
                break;
            case HotkeyAction.ShowMusicPage:
                _island.ShowPage(ExpandedPage.Music);
                break;
            case HotkeyAction.CloseChat:
                _island.CloseChat();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, "Unknown hotkey action.");
        }
    }

    /// <summary>
    /// A click on the pill that no button, the progress bar, a card or the banner took (FR-2): the
    /// compact clock or music pill opens (v1.1: the clock too, FR-23, FR-24), the open island closes.
    /// </summary>
    private void OnPillClicked(object sender, MouseButtonEventArgs e)
    {
        switch (_island.View)
        {
            case IslandView.Compact or IslandView.Music:
                _island.ToggleExpanded(DateTimeOffset.Now);
                break;
            case IslandView.Expanded:
                _island.Collapse(DateTimeOffset.Now);
                break;
            default:
                return;
        }

        e.Handled = true;
    }

    /// <summary>
    /// Mouse wheel or touchpad over the open island: one gesture moves one page (FR-21). Windows
    /// sends the wheel to the window under the pointer, so the island gets it without focus (U9).
    /// </summary>
    private void OnPillMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (_island.View != IslandView.Expanded)
        {
            return;
        }

        e.Handled = true;
        if (_pageScroll.Step(e.Delta, DateTimeOffset.Now) is ExpandedPage page)
        {
            _island.ShowPage(page);
        }
    }

    private void ShowCurrentState()
    {
        ConflictLabel.Text = _island.HotkeyConflictText ?? string.Empty;
        _musicPresenter.Refresh();
        _notificationPresenter.Refresh();
        ClockUnreadDot.SetShown(_island.HasUnseenNotifications);
        MusicLayer.SetUnread(_island.HasUnseenNotifications);

        IslandView view = _island.View;
        PillShape shape = PillShape.For(view);
        UpdatePillColors();
        // Slide only between pages of an island that is already open; otherwise jump to the page.
        ExpandedLayer.ShowPage(_island.Page, slide: _shownView == IslandView.Expanded && view == IslandView.Expanded);

        if (view != _shownView)
        {
            if (view == IslandView.Expanded)
            {
                ExpandedLayer.OnOpened();
            }
            else if (_shownView == IslandView.Expanded)
            {
                ExpandedLayer.OnClosed();
            }

            _animator.SwapContent(LayerFor(_shownView), LayerFor(view));
            _shownView = view;
        }

        if (shape != _shownShape)
        {
            _animator.MorphTo(shape);
            _shownShape = shape;
        }

        if (_island.IsVisible != _shownVisible)
        {
            _animator.SetVisible(_island.IsVisible);
            _shownVisible = _island.IsVisible;
        }
    }

    /// <summary>The artwork color behind the open music page; black everywhere else (FR-25).</summary>
    private void UpdatePillColors()
    {
        ArtworkPalette palette = _musicPresenter.CurrentPalette;
        bool onMusicPage = _island.View == IslandView.Expanded && _island.Page == ExpandedPage.Music;
        _pillColors.Show(onMusicPage && palette != ArtworkPalette.Black, palette);
    }

    private FrameworkElement LayerFor(IslandView view)
    {
        return view switch
        {
            IslandView.Compact => ClockLayer,
            IslandView.HotkeyConflict => ConflictLayer,
            IslandView.Music => MusicLayer,
            IslandView.Expanded => ExpandedLayer,
            IslandView.Notification => BannerLayer,
            IslandView.PermissionPrompt => PermissionLayer,
            _ => throw new ArgumentOutOfRangeException(nameof(view), view, "No layer exists for this view."),
        };
    }

    private void PlaceOnPrimaryDisplay()
    {
        // A display change in the middle of a resize: end the resize first, at the new place.
        _resizeAnimator.FinishNow();
        ApplyIslandScale();

        // The window always has room for the largest island size, so changing the size never
        // resizes or moves the window: resizing it showed as a jump to the side (Diva, 2 Nov 2026).
        IslandLayout window = LayoutFor(IslandSize.ExtraLarge);
        Width = window.Width;
        Height = window.Height;
        Left = window.Left;
        Top = window.Top;

        // Display changes are one moment Windows can rearrange windows; check the island is still on top.
        _topmostGuard?.CheckNow();
    }

    private IslandLayout LayoutFor(IslandSize size)
    {
        // The window lives on the primary monitor, so its DPI is that monitor's DPI.
        double displayWidthDips = PrimaryDisplay.WidthInPixels() / PixelsPerDip();
        return IslandLayout.ForPrimaryDisplay(displayWidthDips, size);
    }

    private void ApplyIslandScale()
    {
        double scale = LayoutFor(_size).Scale;
        IslandScaleTransform.ScaleX = scale;
        IslandScaleTransform.ScaleY = scale;
    }

    private double PixelsPerDip()
    {
        return VisualTreeHelper.GetDpi(this).DpiScaleX;
    }

    private void OnDisplaySettingsChanged(object? sender, EventArgs e)
    {
        Dispatcher.BeginInvoke(PlaceOnPrimaryDisplay);
    }

    private IntPtr OnWindowMessage(IntPtr windowHandle, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (NonActivatingWindow.TryHandleMessage(message, out IntPtr result))
        {
            handled = true;
            return result;
        }

        if (Hotkeys is not null && Hotkeys.Global.TryHandleMessage(message, wParam))
        {
            handled = true;
        }

        return IntPtr.Zero;
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        // SystemEvents keeps static references, so unsubscribe to avoid keeping the window alive.
        SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
        _conflictTimer.Stop();
        _musicPresenter.Dispose();
        _leaveCollapser.Dispose();
        _outsideClickWatcher.Dispose();
        _permissionPrompt.Dispose();
        _expandedKeys?.Dispose();
        _minuteTimer.Dispose();
        _topmostGuard?.Dispose();
        Hotkeys?.Dispose();
    }
}
