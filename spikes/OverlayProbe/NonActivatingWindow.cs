// NonActivatingWindow.cs: the technique PRD FR-3 wants to prove. Makes our window stay on top
// and receive clicks without ever becoming the active window, so the game keeps keyboard focus.

namespace OverlayProbe;

internal static class NonActivatingWindow
{
    public static void Apply(IntPtr windowHandle)
    {
        long style = NativeMethods.GetWindowLongPtr(windowHandle, NativeMethods.GwlExStyle).ToInt64();
        // NoActivate: clicks do not activate us. ToolWindow: no taskbar or Alt+Tab entry.
        style |= NativeMethods.WsExNoActivate | NativeMethods.WsExToolWindow | NativeMethods.WsExTopmost;
        NativeMethods.SetWindowLongPtr(windowHandle, NativeMethods.GwlExStyle, new IntPtr(style));
        BringToTopWithoutActivating(windowHandle);
    }

    public static void BringToTopWithoutActivating(IntPtr windowHandle)
    {
        NativeMethods.SetWindowPos(windowHandle, NativeMethods.HwndTopmost, 0, 0, 0, 0,
            NativeMethods.SwpNoMove | NativeMethods.SwpNoSize | NativeMethods.SwpNoActivate);
    }

    // Window message hook: answers "do not activate me" when the mouse clicks our window.
    // A second line of defence next to the NoActivate style.
    public static IntPtr RefuseMouseActivation(IntPtr windowHandle, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (message == NativeMethods.WmMouseActivate)
        {
            handled = true;
            return new IntPtr(NativeMethods.MaNoActivate);
        }

        return IntPtr.Zero;
    }
}
