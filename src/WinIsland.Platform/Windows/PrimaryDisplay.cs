// PrimaryDisplay.cs: reads the width of the primary monitor in physical pixels.
// The island always sits on the primary monitor (PRD FR-1). The app converts this to DIPs with
// the window's own DPI, which stays correct when Windows display scaling changes while running.

namespace WinIsland.Platform.Windows;

public static class PrimaryDisplay
{
    public static int WidthInPixels()
    {
        int width = NativeMethods.GetSystemMetrics(NativeMethods.SmCxScreen);
        if (width <= 0)
        {
            throw new InvalidOperationException("Windows did not report a primary display width.");
        }

        return width;
    }
}
