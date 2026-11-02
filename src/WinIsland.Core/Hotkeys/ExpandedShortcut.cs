// ExpandedShortcut.cs: the keyboard shortcuts that work only while the island is expanded.
// Music (Diva, 31 Oct 2026, Spotify's own keys): Space plays or pauses, Ctrl+Right goes to the
// next song, Ctrl+Left to the previous one.
// Pages (PRD FR-21, v1.1): Alt+Down shows the notifications page, Alt+Up the music page.
// Chats (PRD FR-35, v1.4): Alt+Left goes from an open chat back to the list. It is taken only
// while a chat is open, so the rest of the time games and browsers keep it (Alt+Left is "back").
// Because the island never has keyboard focus (FR-3), these are global hotkeys, registered when
// the island opens and released when it closes, so the rest of the time the keys belong to the game.

namespace WinIsland.Core.Hotkeys;

public sealed record ExpandedShortcut(HotkeyAction Action, HotkeyModifiers Modifiers, ShortcutKey Key)
{
    public static IReadOnlyList<ExpandedShortcut> All { get; } =
    [
        new(HotkeyAction.MusicPlayPause, HotkeyModifiers.None, ShortcutKey.Space),
        new(HotkeyAction.MusicNext, HotkeyModifiers.Ctrl, ShortcutKey.RightArrow),
        new(HotkeyAction.MusicPrevious, HotkeyModifiers.Ctrl, ShortcutKey.LeftArrow),
        new(HotkeyAction.ShowNotificationsPage, HotkeyModifiers.Alt, ShortcutKey.DownArrow),
        new(HotkeyAction.ShowMusicPage, HotkeyModifiers.Alt, ShortcutKey.UpArrow),
    ];

    /// <summary>Only while a chat is open (FR-35).</summary>
    public static ExpandedShortcut BackToList { get; } = new(HotkeyAction.CloseChat, HotkeyModifiers.Alt, ShortcutKey.LeftArrow);

    public override string ToString()
    {
        string key = Key switch
        {
            ShortcutKey.Space => "Space",
            ShortcutKey.LeftArrow => "Left",
            ShortcutKey.RightArrow => "Right",
            ShortcutKey.UpArrow => "Up",
            ShortcutKey.DownArrow => "Down",
            _ => throw new ArgumentOutOfRangeException(nameof(Key), Key, "Unknown shortcut key."),
        };

        string prefix = string.Empty;
        if (Modifiers.HasFlag(HotkeyModifiers.Ctrl))
        {
            prefix += "Ctrl+";
        }

        if (Modifiers.HasFlag(HotkeyModifiers.Alt))
        {
            prefix += "Alt+";
        }

        return prefix + key;
    }
}
