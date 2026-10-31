// FocusCheck.cs: measures whether a click on the pill moved keyboard focus away from the game.
// It compares which window is in front just before the click with which window is in front
// just after. Only window handles are compared; nothing is read from the game.

namespace OverlayProbe;

internal sealed class FocusCheck
{
    private IntPtr _foregroundBeforeClick;

    public IntPtr OverlayHandle { get; set; }

    public void RememberForegroundBeforeClick()
    {
        _foregroundBeforeClick = NativeMethods.GetForegroundWindow();
    }

    public string DescribeAfterClick()
    {
        IntPtr foregroundNow = NativeMethods.GetForegroundWindow();
        if (foregroundNow == OverlayHandle)
        {
            return "NO (the pill took focus)";
        }

        if (foregroundNow != _foregroundBeforeClick)
        {
            return "NO (focus moved to another window)";
        }

        return "YES";
    }
}
