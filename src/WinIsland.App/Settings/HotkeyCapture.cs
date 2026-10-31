// HotkeyCapture.cs: turns a key press in the Settings window into a hotkey.
// Only letters and digits count as the main key (the hotkey rule in Core); pressing just a
// modifier (Ctrl, Alt, Shift, Win) waits for the main key. With Alt held, WPF reports the key as
// "System", so the real key is read from SystemKey.

using System.Windows.Input;

using WinIsland.Core.Hotkeys;

namespace WinIsland.App.Settings;

internal static class HotkeyCapture
{
    public static Hotkey? FromKeyPress(KeyEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        Key key = e.Key == Key.System ? e.SystemKey : e.Key;
        char? main = key switch
        {
            >= Key.A and <= Key.Z => (char)('A' + (key - Key.A)),
            >= Key.D0 and <= Key.D9 => (char)('0' + (key - Key.D0)),
            _ => null,
        };

        if (main is null)
        {
            return null;
        }

        return new Hotkey(ModifiersFrom(Keyboard.Modifiers), main.Value);
    }

    private static HotkeyModifiers ModifiersFrom(ModifierKeys keys)
    {
        HotkeyModifiers result = HotkeyModifiers.None;
        if (keys.HasFlag(ModifierKeys.Control))
        {
            result |= HotkeyModifiers.Ctrl;
        }

        if (keys.HasFlag(ModifierKeys.Alt))
        {
            result |= HotkeyModifiers.Alt;
        }

        if (keys.HasFlag(ModifierKeys.Shift))
        {
            result |= HotkeyModifiers.Shift;
        }

        if (keys.HasFlag(ModifierKeys.Windows))
        {
            result |= HotkeyModifiers.Win;
        }

        return result;
    }
}
