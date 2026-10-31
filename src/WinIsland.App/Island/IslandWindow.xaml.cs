// IslandWindow.xaml.cs: connects the island window to the island's rules.
// Makes the window non-activating (FR-3), places and scales it on the primary monitor (FR-1),
// registers the hotkeys (FR-5), shows the conflict banner for 10 seconds when a hotkey is taken,
// shows the clock (AC-1), shows music and opens or closes on click (FR-2, FR-7), and animates
// whenever the island's state changes (FR-4).

using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

using Microsoft.Win32;

using WinIsland.App.Clock;
using WinIsland.App.Music;
using WinIsland.Core.Hotkeys;
using WinIsland.Core.Island;
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
    private readonly IslandMusicPresenter _musicPresenter;
    private readonly PointerLeaveCollapser _leaveCollapser;
    private GlobalHotkeys? _hotkeys;
    private IslandView _shownView = IslandView.Compact;
    private bool _shownVisible = true;

    internal IslandWindow(IslandStateMachine island, MusicController music)
    {
        ArgumentNullException.ThrowIfNull(island);
        ArgumentNullException.ThrowIfNull(music);
        _island = island;

        InitializeComponent();
        _animator = new PillAnimator(Pill, PillScale, PillOffset);
        _animator.SetShape(PillShape.For(IslandView.Compact));
        _musicPresenter = new IslandMusicPresenter(island, music, MusicLayer, ExpandedLayer);
        _leaveCollapser = new PointerLeaveCollapser(island, Pill);

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

    private void RegisterHotkeys(IntPtr handle)
    {
        _hotkeys = new GlobalHotkeys(handle);
        _hotkeys.Pressed += OnHotkeyPressed;

        // Hotkey 3 (hide the notification banner) is registered once banners exist (Phase 4),
        // so it does not take Alt+X away from games before it does anything.
        Dictionary<HotkeyAction, Hotkey> wanted = new()
        {
            [HotkeyAction.Expand] = HotkeySet.Defaults.Expand,
            [HotkeyAction.HideShow] = HotkeySet.Defaults.HideShow,
        };

        IReadOnlyList<HotkeyRegistration> results = _hotkeys.Register(wanted);
        List<Hotkey> unavailable = results.Where(result => !result.IsRegistered).Select(result => result.Hotkey).ToList();
        string? conflictMessage = HotkeyConflictMessage.Describe(unavailable);
        if (conflictMessage is not null)
        {
            _island.ShowHotkeyConflict(conflictMessage);
            _conflictTimer.Start();
        }
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
                // Not registered until notification banners exist (Phase 4).
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, "Unknown hotkey action.");
        }
    }

    /// <summary>
    /// A click on the pill that no button or the progress bar took (FR-2): the compact music pill
    /// opens, the open island closes. A click on the clock does nothing.
    /// </summary>
    private void OnPillClicked(object sender, MouseButtonEventArgs e)
    {
        switch (_island.View)
        {
            case IslandView.Music:
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

    private void ShowCurrentState()
    {
        ConflictLabel.Text = _island.HotkeyConflictText ?? string.Empty;
        _musicPresenter.Refresh();

        if (_island.View != _shownView)
        {
            _animator.MorphTo(PillShape.For(_island.View));
            _animator.SwapContent(LayerFor(_shownView), LayerFor(_island.View));
            _shownView = _island.View;
        }

        if (_island.IsVisible != _shownVisible)
        {
            _animator.SetVisible(_island.IsVisible);
            _shownVisible = _island.IsVisible;
        }
    }

    private FrameworkElement LayerFor(IslandView view)
    {
        return view switch
        {
            IslandView.Compact => ClockLayer,
            IslandView.HotkeyConflict => ConflictLayer,
            IslandView.Music => MusicLayer,
            IslandView.Expanded => ExpandedLayer,
            _ => throw new ArgumentOutOfRangeException(nameof(view), view, "No layer exists for this view."),
        };
    }

    private void PlaceOnPrimaryDisplay()
    {
        // The window lives on the primary monitor, so its DPI is that monitor's DPI.
        double displayWidthDips = PrimaryDisplay.WidthInPixels() / PixelsPerDip();
        IslandLayout layout = IslandLayout.ForPrimaryDisplay(displayWidthDips);

        IslandScaleTransform.ScaleX = layout.Scale;
        IslandScaleTransform.ScaleY = layout.Scale;
        Width = layout.Width;
        Height = layout.Height;
        Left = layout.Left;
        Top = layout.Top;
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

        if (_hotkeys is not null && _hotkeys.TryHandleMessage(message, wParam))
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
        _minuteTimer.Dispose();
        _hotkeys?.Dispose();
    }
}
