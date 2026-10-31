// TrayMenuLabelsTests.cs: checks the tray menu wording for each state (PRD FR-6, design board).

using WinIsland.Core.Hotkeys;
using WinIsland.Core.Tray;

namespace WinIsland.Core.Tests.Tray;

public class TrayMenuLabelsTests
{
    [Fact]
    public void Pause_item_reads_Resume_while_paused()
    {
        Assert.Equal("Pause for 1 hour", TrayMenuLabels.PauseOrResume(isPaused: false));
        Assert.Equal("Resume", TrayMenuLabels.PauseOrResume(isPaused: true));
    }

    [Fact]
    public void Hide_item_shows_the_hotkey_and_reads_Show_while_hidden()
    {
        Hotkey hotkey = HotkeySet.Defaults.HideShow;

        Assert.Equal("Hide island (Ctrl+Alt+H)", TrayMenuLabels.HideOrShow(isHidden: false, hotkey));
        Assert.Equal("Show island (Ctrl+Alt+H)", TrayMenuLabels.HideOrShow(isHidden: true, hotkey));
    }

    [Fact]
    public void Hide_item_follows_a_changed_hotkey()
    {
        Hotkey changed = new(HotkeyModifiers.Ctrl | HotkeyModifiers.Shift, 'J');

        Assert.Equal("Hide island (Ctrl+Shift+J)", TrayMenuLabels.HideOrShow(isHidden: false, changed));
    }

    [Fact]
    public void Hide_item_needs_a_hotkey()
    {
        Assert.Throws<ArgumentNullException>(() => TrayMenuLabels.HideOrShow(isHidden: false, null!));
    }
}
