// Hotkey.cs: one key combination, for example Ctrl+Alt+I.
// Only letters and digits are allowed as the main key, which covers the PRD defaults and keeps
// settings simple. The text form ("Ctrl+Alt+I") is what the user sees in banners and settings.

namespace WinIsland.Core.Hotkeys;

public sealed record Hotkey(HotkeyModifiers Modifiers, char Key)
{
    public bool HasModifier => Modifiers != HotkeyModifiers.None;

    public bool HasValidKey => char.IsAsciiLetterOrDigit(Key);

    public override string ToString()
    {
        List<string> parts = [];
        if (Modifiers.HasFlag(HotkeyModifiers.Ctrl))
        {
            parts.Add("Ctrl");
        }

        if (Modifiers.HasFlag(HotkeyModifiers.Alt))
        {
            parts.Add("Alt");
        }

        if (Modifiers.HasFlag(HotkeyModifiers.Shift))
        {
            parts.Add("Shift");
        }

        if (Modifiers.HasFlag(HotkeyModifiers.Win))
        {
            parts.Add("Win");
        }

        parts.Add(char.ToUpperInvariant(Key).ToString());
        return string.Join("+", parts);
    }

    /// <summary>Two hotkeys are the same combination even if one key was typed in lower case.</summary>
    public bool IsSameCombinationAs(Hotkey other)
    {
        return Modifiers == other.Modifiers && char.ToUpperInvariant(Key) == char.ToUpperInvariant(other.Key);
    }
}
