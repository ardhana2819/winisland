// BlurWindow.cs: the empty window right behind the island that carries the Glass blur (PRD FR-37).
// It has no content of its own: Windows' blur (GlassBackdrop in Platform) would hide WPF content
// in the same window, which is why the island cannot carry it itself (glass spike round 7).
// It never takes focus, clicks pass through it, and it stays on top like the island.
// If Windows refuses the blur, Backdrop stays null and Problem says why; the island then shows Solid.

using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

using WinIsland.Platform.Windows;

namespace WinIsland.App.Island;

internal sealed class BlurWindow : Window
{
    private TopmostGuard? _topmostGuard;

    public BlurWindow()
    {
        Title = "WinIsland glass";
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        AllowsTransparency = false;
        Background = Brushes.Transparent;
        Topmost = true;
        ShowInTaskbar = false;
        ShowActivated = false;
        Focusable = false;
        SourceInitialized += OnSourceInitialized;
        Closed += (_, _) => _topmostGuard?.Dispose();
    }

    public GlassBackdrop? Backdrop { get; private set; }

    /// <summary>Why the blur could not start, or null.</summary>
    public Exception? Problem { get; private set; }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        IntPtr handle = new WindowInteropHelper(this).Handle;
        HwndSource.FromHwnd(handle).CompositionTarget.BackgroundColor = Colors.Transparent;
        try
        {
            Backdrop = new GlassBackdrop(handle);
            _topmostGuard = new TopmostGuard(handle);
        }
        catch (Exception error)
        {
            // Reported to the caller, which logs it and falls back to Solid (PRD FR-37).
            Problem = error;
        }
    }
}
