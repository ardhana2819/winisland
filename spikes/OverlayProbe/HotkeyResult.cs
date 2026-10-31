// HotkeyResult.cs: the outcome of registering one hotkey, and how often it was pressed.
// Shown on the pill so Diva can read the result while a game is running (U5).

namespace OverlayProbe;

internal sealed class HotkeyResult(HotkeyDefinition hotkey, bool registered, int errorCode)
{
    // 1409 = ERROR_HOTKEY_ALREADY_REGISTERED: another program already owns this combination.
    private const int HotkeyAlreadyRegistered = 1409;

    public HotkeyDefinition Hotkey { get; } = hotkey;

    public int PressCount { get; set; }

    public string Describe()
    {
        if (registered)
        {
            return $"{Hotkey.Label}: OK, pressed {PressCount}x";
        }

        return errorCode == HotkeyAlreadyRegistered
            ? $"{Hotkey.Label}: CONFLICT (taken by another app)"
            : $"{Hotkey.Label}: FAILED (error {errorCode})";
    }
}
