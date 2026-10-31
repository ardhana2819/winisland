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

    /// <summary>Reads text like "Ctrl+Alt+I" (as written by ToString). Returns null when it is not a hotkey.</summary>
    public static Hotkey? TryParse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        string[] parts = text.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0 || parts[^1].Length != 1)
        {
            return null;
        }

        HotkeyModifiers modifiers = HotkeyModifiers.None;
        foreach (string part in parts[..^1])
        {
            HotkeyModifiers? modifier = part.ToUpperInvariant() switch
            {
                "CTRL" => HotkeyModifiers.Ctrl,
                "ALT" => HotkeyModifiers.Alt,
                "SHIFT" => HotkeyModifiers.Shift,
                "WIN" => HotkeyModifiers.Win,
                _ => null,
            };

            if (modifier is null)
            {
                return null;
            }

            modifiers |= modifier.Value;
        }

        Hotkey hotkey = new(modifiers, char.ToUpperInvariant(parts[^1][0]));
        return hotkey.HasValidKey ? hotkey : null;
    }

    /// <summary>Two hotkeys are the same combination even if one key was typed in lower case.</summary>
    public bool IsSameCombinationAs(Hotkey other)
    {
        return Modifiers == other.Modifiers && char.ToUpperInvariant(Key) == char.ToUpperInvariant(other.Key);
    }
}
