// HotkeyConflictMessage.cs: writes the short message for the hotkey conflict banner (PRD FR-5).
// The banner is only 400 wide (DESIGN_SPEC section 2), so the text stays short, like the
// design board example "Alt+X is used by another app".

namespace WinIsland.Core.Hotkeys;

public static class HotkeyConflictMessage
{
    /// <summary>Returns the banner text, or null when every hotkey could be turned on.</summary>
    public static string? Describe(IReadOnlyList<Hotkey> unavailableHotkeys)
    {
        ArgumentNullException.ThrowIfNull(unavailableHotkeys);

        return unavailableHotkeys.Count switch
        {
            0 => null,
            1 => $"{unavailableHotkeys[0]} is used by another app",
            _ => $"{string.Join(" and ", unavailableHotkeys.Select(hotkey => hotkey.ToString()))} are used by other apps",
        };
    }
}
