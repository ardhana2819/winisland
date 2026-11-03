// ExpandedShortcutTests.cs: checks the keys that work while the island is expanded: the music keys
// Diva asked for (Spotify's own: Space, Ctrl+Right, Ctrl+Left) and the page keys (Alt+Down, Alt+Up, FR-21).

using WinIsland.Core.Hotkeys;

namespace WinIsland.Core.Tests.Hotkeys;

public class ExpandedShortcutTests
{
    [Fact]
    public void Shortcuts_match_Spotify_keys_and_the_page_keys()
    {
        Dictionary<HotkeyAction, string> labels = ExpandedShortcut.All.ToDictionary(shortcut => shortcut.Action, shortcut => shortcut.ToString());

        Assert.Equal("Space", labels[HotkeyAction.MusicPlayPause]);
        Assert.Equal("Ctrl+Right", labels[HotkeyAction.MusicNext]);
        Assert.Equal("Ctrl+Left", labels[HotkeyAction.MusicPrevious]);
        Assert.Equal("Alt+Down", labels[HotkeyAction.ShowNotificationsPage]);
        Assert.Equal("Alt+Up", labels[HotkeyAction.ShowMusicPage]);
        Assert.Equal("Ctrl+Up", labels[HotkeyAction.VolumeUp]);
        Assert.Equal("Ctrl+Down", labels[HotkeyAction.VolumeDown]);
    }

    [Fact]
    public void Back_from_a_chat_is_Alt_Left_and_not_one_of_the_always_on_keys()
    {
        Assert.Equal("Alt+Left", ExpandedShortcut.BackToList.ToString());
        Assert.Equal(HotkeyAction.CloseChat, ExpandedShortcut.BackToList.Action);
        Assert.DoesNotContain(ExpandedShortcut.BackToList, ExpandedShortcut.All);
    }

    [Fact]
    public void Each_shortcut_has_its_own_action_and_none_clashes_with_the_island_hotkeys()
    {
        Assert.Equal(ExpandedShortcut.All.Count, ExpandedShortcut.All.Select(shortcut => shortcut.Action).Distinct().Count());
        Assert.DoesNotContain(ExpandedShortcut.All, shortcut =>
            shortcut.Action is HotkeyAction.Expand or HotkeyAction.HideShow or HotkeyAction.HideBanner);
    }
}
