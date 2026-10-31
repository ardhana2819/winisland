// PillWindow.xaml.cs: wires the test pill together.
// On load it applies the non-activating window style, places the pill at the top center,
// registers the hotkeys, and then waits. No timers run unless --reassert-topmost is given,
// so idle CPU measures the window itself.

using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace OverlayProbe;

public partial class PillWindow : Window
{
    private const double TopMarginDips = 8;
    private static readonly TimeSpan FocusCheckDelay = TimeSpan.FromMilliseconds(150);
    private static readonly TimeSpan TopmostReassertInterval = TimeSpan.FromSeconds(2);

    private readonly FocusCheck _focusCheck = new();
    private HotkeyRegistrar? _hotkeys;
    private DispatcherTimer? _topmostTimer;
    private int _clickCount;

    public PillWindow()
    {
        InitializeComponent();
        SourceInitialized += OnSourceInitialized;
        Closed += OnClosed;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        IntPtr windowHandle = new WindowInteropHelper(this).Handle;
        NonActivatingWindow.Apply(windowHandle);
        _focusCheck.OverlayHandle = windowHandle;

        Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
        Top = TopMarginDips;

        HwndSource source = HwndSource.FromHwnd(windowHandle);
        source.AddHook(NonActivatingWindow.RefuseMouseActivation);

        _hotkeys = new HotkeyRegistrar(source, OnHotkeyPressed);
        _hotkeys.RegisterAll();
        ShowHotkeyStatus();

        bool reassertTopmost = Environment.GetCommandLineArgs().Contains("--reassert-topmost");
        ModeStatus.Text = reassertTopmost
            ? "Mode: re-asserting topmost every 2 s. Right-click to close."
            : "Mode: plain topmost. Right-click to close.";
        if (reassertTopmost)
        {
            StartTopmostReassert(windowHandle);
        }
    }

    private void StartTopmostReassert(IntPtr windowHandle)
    {
        _topmostTimer = new DispatcherTimer { Interval = TopmostReassertInterval };
        _topmostTimer.Tick += (_, _) => NonActivatingWindow.BringToTopWithoutActivating(windowHandle);
        _topmostTimer.Start();
    }

    private void OnPillMouseEnter(object sender, MouseEventArgs e)
    {
        _focusCheck.RememberForegroundBeforeClick();
    }

    private async void OnPillClicked(object sender, MouseButtonEventArgs e)
    {
        _clickCount++;
        // Give Windows a moment to move focus, if it is going to.
        await Task.Delay(FocusCheckDelay);
        ClickStatus.Text = $"Clicks: {_clickCount} | Focus kept: {_focusCheck.DescribeAfterClick()}";
    }

    private void OnPillRightClicked(object sender, MouseButtonEventArgs e)
    {
        Close();
    }

    private void OnHotkeyPressed(HotkeyDefinition hotkey)
    {
        ShowHotkeyStatus();
    }

    private void ShowHotkeyStatus()
    {
        if (_hotkeys is not null)
        {
            HotkeyStatus.Text = string.Join("   ", _hotkeys.Results.Select(result => result.Describe()));
        }
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _topmostTimer?.Stop();
        _hotkeys?.UnregisterAll();
    }
}
