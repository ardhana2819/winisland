// MusicShortcutTests.cs: checks the music keys Diva asked for (Spotify's own: Space, Ctrl+Right, Ctrl+Left).

using WinIsland.Core.Hotkeys;

namespace WinIsland.Core.Tests.Hotkeys;

public class MusicShortcutTests
{
    [Fact]
    public void Shortcuts_match_Spotify_keys()
    {
        Dictionary<HotkeyAction, string> labels = MusicShortcut.All.ToDictionary(shortcut => shortcut.Action, shortcut => shortcut.ToString());

        Assert.Equal("Space", labels[HotkeyAction.MusicPlayPause]);
        Assert.Equal("Ctrl+Right", labels[HotkeyAction.MusicNext]);
        Assert.Equal("Ctrl+Left", labels[HotkeyAction.MusicPrevious]);
    }

    [Fact]
    public void Each_shortcut_has_its_own_action_and_none_clashes_with_the_island_hotkeys()
    {
        Assert.Equal(MusicShortcut.All.Count, MusicShortcut.All.Select(shortcut => shortcut.Action).Distinct().Count());
        Assert.DoesNotContain(MusicShortcut.All, shortcut =>
            shortcut.Action is HotkeyAction.Expand or HotkeyAction.HideShow or HotkeyAction.HideBanner);
    }
}
