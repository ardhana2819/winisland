// IslandWindow.xaml.cs: sets up the island window.
// Makes it non-activating (FR-3), places and scales it on the primary monitor (FR-1), keeps it
// in place when the display changes, and shows the clock (AC-1).

using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

using Microsoft.Win32;

using WinIsland.App.Clock;
using WinIsland.Core.Island;
using WinIsland.Platform.Windows;

namespace WinIsland.App.Island;

public partial class IslandWindow : Window
{
    private readonly MinuteTimer _minuteTimer = new();

    public IslandWindow()
    {
        InitializeComponent();
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
    }

    private void PlaceOnPrimaryDisplay()
    {
        // The window lives on the primary monitor, so its DPI is that monitor's DPI.
        double pixelsPerDip = PixelsPerDip();
        double displayWidthDips = PrimaryDisplay.WidthInPixels() / pixelsPerDip;
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

        return IntPtr.Zero;
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        // SystemEvents keeps static references, so unsubscribe to avoid keeping the window alive.
        SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
        _minuteTimer.Dispose();
    }
}
