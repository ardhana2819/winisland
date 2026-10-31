// PointerState.cs: reads where the mouse pointer is and whether a mouse button is down.
// Used only while the island is expanded, so a click anywhere outside it can close it (Diva's
// request, 31 Oct 2026). It reads the current state and nothing more: no hook, nothing installed
// into other programs (PRD section 7), and the click still reaches the game as normal.

namespace WinIsland.Platform.Windows;

public static class PointerState
{
    // The highest bit of GetAsyncKeyState's answer means "down right now".
    private const short DownBit = unchecked((short)0x8000);

    public static bool IsAnyButtonDown()
    {
        return IsDown(NativeMethods.VkLeftButton)
            || IsDown(NativeMethods.VkRightButton)
            || IsDown(NativeMethods.VkMiddleButton);
    }

    /// <summary>The pointer position in physical screen pixels, or null if Windows does not say.</summary>
    public static (int X, int Y)? Position()
    {
        return NativeMethods.GetCursorPos(out NativeMethods.Point point) ? (point.X, point.Y) : null;
    }

    private static bool IsDown(int virtualKey)
    {
        return (NativeMethods.GetAsyncKeyState(virtualKey) & DownBit) != 0;
    }
}
