// HotkeySetTests.cs: checks the default hotkeys (PRD FR-5) and the rule that two hotkeys cannot be the same (section 12).

using WinIsland.Core.Hotkeys;

namespace WinIsland.Core.Tests.Hotkeys;

public class HotkeySetTests
{
    [Fact]
    public void Defaults_match_the_PRD_and_are_valid()
    {
        HotkeySet defaults = HotkeySet.Defaults;

        Assert.Equal("Ctrl+Alt+I", defaults.Expand.ToString());
        Assert.Equal("Ctrl+Alt+H", defaults.HideShow.ToString());
        Assert.Equal("Alt+X", defaults.HideBanner.ToString());
        Assert.Empty(defaults.FindProblems());
    }

    [Fact]
    public void Two_hotkeys_with_the_same_combination_are_reported()
    {
        HotkeySet set = HotkeySet.Defaults with { HideBanner = new Hotkey(HotkeyModifiers.Ctrl | HotkeyModifiers.Alt, 'i') };

        string problem = Assert.Single(set.FindProblems());
        Assert.Contains("Expand island and Hide notifications both use Ctrl+Alt+I", problem);
    }

    [Fact]
    public void All_three_the_same_reports_every_pair()
    {
        Hotkey same = new(HotkeyModifiers.Alt, 'X');
        HotkeySet set = new(same, same, same);

        Assert.Equal(3, set.FindProblems().Count);
    }

    [Fact]
    public void A_hotkey_without_modifier_is_reported()
    {
        HotkeySet set = HotkeySet.Defaults with { HideBanner = new Hotkey(HotkeyModifiers.None, 'X') };

        string problem = Assert.Single(set.FindProblems());
        Assert.StartsWith("Hide notifications: add Ctrl, Alt, Shift or Win", problem);
    }

    [Fact]
    public void A_hotkey_with_an_invalid_key_is_reported()
    {
        HotkeySet set = HotkeySet.Defaults with { Expand = new Hotkey(HotkeyModifiers.Ctrl, '-') };

        string problem = Assert.Single(set.FindProblems());
        Assert.StartsWith("Expand island: use a letter or a digit", problem);
    }
}
