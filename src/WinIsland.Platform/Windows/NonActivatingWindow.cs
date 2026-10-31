// NonActivatingWindow.cs: makes a window stay on top and accept clicks without ever becoming
// the active window, so the game keeps keyboard focus (PRD FR-3).
// This is the technique spike U4 proved in Forza Horizon 6, Honkai Star Rail and Zenless Zone Zero.

using System.ComponentModel;
using System.Runtime.InteropServices;

namespace WinIsland.Platform.Windows;

public static class NonActivatingWindow
{
    public static void Apply(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
        {
            throw new ArgumentException("The window has no handle yet. Call this after the window is created.", nameof(windowHandle));
        }

        long style = NativeMethods.GetWindowLongPtr(windowHandle, NativeMethods.GwlExStyle).ToInt64();
        // NoActivate: clicks do not activate us. ToolWindow: no taskbar or Alt+Tab entry.
        style |= NativeMethods.WsExNoActivate | NativeMethods.WsExToolWindow | NativeMethods.WsExTopmost;

        Marshal.SetLastPInvokeError(0);
        IntPtr previous = NativeMethods.SetWindowLongPtr(windowHandle, NativeMethods.GwlExStyle, new IntPtr(style));
        int error = Marshal.GetLastPInvokeError();
        if (previous == IntPtr.Zero && error != 0)
        {
            throw new Win32Exception(error, "Could not make the island window non-activating.");
        }

        bool placed = NativeMethods.SetWindowPos(windowHandle, NativeMethods.HwndTopmost, 0, 0, 0, 0,
            NativeMethods.SwpNoMove | NativeMethods.SwpNoSize | NativeMethods.SwpNoActivate);
        if (!placed)
        {
            throw new Win32Exception(Marshal.GetLastPInvokeError(), "Could not place the island window on top.");
        }
    }

    /// <summary>
    /// Call from the window's message hook. Answers "do not activate me" when the mouse clicks
    /// the window, a second line of defence next to the NoActivate style.
    /// </summary>
    public static bool TryHandleMessage(int message, out IntPtr result)
    {
        if (message == NativeMethods.WmMouseActivate)
        {
            result = new IntPtr(NativeMethods.MaNoActivate);
            return true;
        }

        result = IntPtr.Zero;
        return false;
    }
}
