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
    /// Checks that the window still has its on-top, no-activate and tool-window styles, and puts
    /// back whatever is missing. Returns the names of the styles that were missing, or an empty
    /// list when everything was in place. Diva saw the island fall behind Chrome and Spotify after
    /// closing and reopening apps (31 Oct 2026); this is the repair for that.
    /// </summary>
    public static IReadOnlyList<string> RepairIfNeeded(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
        {
            return [];
        }

        long style = NativeMethods.GetWindowLongPtr(windowHandle, NativeMethods.GwlExStyle).ToInt64();
        List<string> missing = [];
        if ((style & NativeMethods.WsExTopmost) == 0)
        {
            missing.Add("topmost");
        }

        if ((style & NativeMethods.WsExNoActivate) == 0)
        {
            missing.Add("no-activate");
        }

        if ((style & NativeMethods.WsExToolWindow) == 0)
        {
            missing.Add("tool-window");
        }

        if (missing.Count > 0)
        {
            Apply(windowHandle);
        }

        return missing;
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
