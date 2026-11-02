// BlurWindow.cs: a second, empty window that only carries the blur (spike round 8).
// Round 7 showed that the host backdrop hides all WPF content of the window it is attached to, so
// the clock vanished. Here the blur lives in its own window right behind the pill window, which
// stays a normal transparent WPF window on top. Like the pill window, this one never takes focus,
// and clicks pass through it. Windows draws the blur; nothing here reads the screen.

using System.Windows;
using System.Windows.Interop;

namespace GlassProbe;

internal sealed class BlurWindow : Window
{
    public BlurWindow(double width, double height)
    {
        Title = "GlassProbe blur";
        Width = width;
        Height = height;
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        AllowsTransparency = false;
        Background = System.Windows.Media.Brushes.Transparent;
        Topmost = true;
        ShowInTaskbar = false;
        ShowActivated = false;
        SourceInitialized += OnSourceInitialized;
    }

    public IntPtr Handle { get; private set; }

    public HostBackdrop? Backdrop { get; private set; }

    public string Status { get; private set; } = "not set";

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        Handle = new WindowInteropHelper(this).Handle;
        long style = NativeMethods.GetWindowLongPtr(Handle, NativeMethods.GwlExStyle).ToInt64();
        long clickThroughNoFocus = NativeMethods.WsExNoActivate | NativeMethods.WsExToolWindow | NativeMethods.WsExTransparent;
        NativeMethods.SetWindowLongPtr(Handle, NativeMethods.GwlExStyle, new IntPtr(style | clickThroughNoFocus));

        NativeMethods.Margins wholeWindow = new() { Left = -1, Right = -1, Top = -1, Bottom = -1 };
        NativeMethods.DwmExtendFrameIntoClientArea(Handle, ref wholeWindow);
        int noRounding = NativeMethods.DwmwcpDoNotRound;
        NativeMethods.DwmSetWindowAttribute(Handle, NativeMethods.DwmwaWindowCornerPreference, ref noRounding, sizeof(int));
        try
        {
            Backdrop = new HostBackdrop(Handle);
            Status = "blur window ready";
        }
        catch (Exception exception)
        {
            // A spike: show the failure on the pill so Diva can read it out.
            Status = $"blur window failed: {exception.GetType().Name} {exception.Message}";
        }
    }
}
