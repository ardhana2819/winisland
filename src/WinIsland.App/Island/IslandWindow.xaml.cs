// IslandWindow.xaml.cs: connects the island window to the island's rules.
// Makes the window non-activating (FR-3), places and scales it on the primary monitor (FR-1),
// registers the hotkeys (FR-5), shows the conflict banner for 10 seconds when a hotkey is taken,
// shows the clock (AC-1), and animates whenever the island's state changes (FR-4).

using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

using Microsoft.Win32;

using WinIsland.App.Clock;
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
    private GlobalHotkeys? _hotkeys;
    private IslandView _shownView = IslandView.Compact;
    private bool _shownVisible = true;

    public IslandWindow(IslandStateMachine island)
    {
        ArgumentNullException.ThrowIfNull(island);
        _island = island;

        InitializeComponent();
        _animator = new PillAnimator(Pill, PillScale, PillOffset);
        _animator.SetShape(PillShape.For(IslandView.Compact));

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

        _minuteTimer.MinuteChanged += (_, now) => ClockLabel.Text = ClockText.Format(now);
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
                _island.ToggleExpanded();
                break;
            case HotkeyAction.HideShow:
                _island.ToggleHidden();
                break;
            case HotkeyAction.HideBanner:
                // Not registered until notification banners exist (Phase 4).
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, "Unknown hotkey action.");
        }
    }

    private void ShowCurrentState()
    {
        ConflictLabel.Text = _island.HotkeyConflictText ?? string.Empty;

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
            // The expanded island needs music, which arrives in Phase 3; until then it cannot be reached.
            IslandView.Expanded => throw new InvalidOperationException("The expanded island has no content until music is built (Phase 3)."),
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
        _minuteTimer.Dispose();
        _hotkeys?.Dispose();
    }
}
