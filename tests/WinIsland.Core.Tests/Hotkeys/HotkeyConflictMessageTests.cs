// HotkeyConflictMessageTests.cs: checks the text of the hotkey conflict banner.

using WinIsland.Core.Hotkeys;

namespace WinIsland.Core.Tests.Hotkeys;

public class HotkeyConflictMessageTests
{
    [Fact]
    public void No_unavailable_hotkeys_means_no_message()
    {
        Assert.Null(HotkeyConflictMessage.Describe([]));
    }

    [Fact]
    public void One_unavailable_hotkey_matches_the_design_board_wording()
    {
        string? message = HotkeyConflictMessage.Describe([HotkeySet.Defaults.HideBanner]);

        Assert.Equal("Alt+X is used by another app", message);
    }

    [Fact]
    public void Several_unavailable_hotkeys_are_listed_together()
    {
        string? message = HotkeyConflictMessage.Describe([HotkeySet.Defaults.Expand, HotkeySet.Defaults.HideShow]);

        Assert.Equal("Ctrl+Alt+I and Ctrl+Alt+H are used by other apps", message);
    }

    [Fact]
    public void A_missing_list_is_rejected()
    {
        Assert.Throws<ArgumentNullException>(() => HotkeyConflictMessage.Describe(null!));
    }
}
