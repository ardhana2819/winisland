// MusicShortcut.cs: the keyboard shortcuts for music, active only while the island is expanded.
// Diva asked for Spotify's own keys (31 Oct 2026): Space plays or pauses, Ctrl+Right goes to the
// next song, Ctrl+Left to the previous one. Because the island never has keyboard focus (FR-3),
// these are global hotkeys, registered when the island opens and released when it closes, so
// the rest of the time the keys belong to the game.

namespace WinIsland.Core.Hotkeys;

public sealed record MusicShortcut(HotkeyAction Action, HotkeyModifiers Modifiers, MusicKey Key)
{
    public static IReadOnlyList<MusicShortcut> All { get; } =
    [
        new(HotkeyAction.MusicPlayPause, HotkeyModifiers.None, MusicKey.Space),
        new(HotkeyAction.MusicNext, HotkeyModifiers.Ctrl, MusicKey.RightArrow),
        new(HotkeyAction.MusicPrevious, HotkeyModifiers.Ctrl, MusicKey.LeftArrow),
    ];

    public override string ToString()
    {
        string key = Key switch
        {
            MusicKey.Space => "Space",
            MusicKey.LeftArrow => "Left",
            MusicKey.RightArrow => "Right",
            _ => throw new ArgumentOutOfRangeException(nameof(Key), Key, "Unknown music key."),
        };

        return Modifiers.HasFlag(HotkeyModifiers.Ctrl) ? $"Ctrl+{key}" : key;
    }
}
