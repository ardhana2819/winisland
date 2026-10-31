// HotkeyModifiers.cs: the modifier keys a hotkey can use.
// Kept in Core (not tied to Windows) so hotkey rules can be tested; Platform translates them
// to the Windows values when it registers the hotkey.

namespace WinIsland.Core.Hotkeys;

[Flags]
public enum HotkeyModifiers
{
    None = 0,
    Ctrl = 1,
    Alt = 2,
    Shift = 4,
    Win = 8,
}
