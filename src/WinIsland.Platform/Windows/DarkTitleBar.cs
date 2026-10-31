// DarkTitleBar.cs: asks Windows 11 to draw a window's title bar dark, so the Settings window
// matches WinIsland's dark-only design (PRD section 8). Only affects WinIsland's own window.

using System.Runtime.InteropServices;

namespace WinIsland.Platform.Windows;

public static partial class DarkTitleBar
{
    // DWMWA_USE_IMMERSIVE_DARK_MODE, available on Windows 11.
    private const int UseImmersiveDarkMode = 20;

    public static void Apply(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
        {
            return;
        }

        int enabled = 1;
        // A failure only means a light title bar; the window still works, so the result is not fatal.
        _ = DwmSetWindowAttribute(windowHandle, UseImmersiveDarkMode, ref enabled, sizeof(int));
    }

    [LibraryImport("dwmapi.dll")]
    private static partial int DwmSetWindowAttribute(IntPtr windowHandle, int attribute, ref int value, int size);
}
