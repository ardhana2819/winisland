// HotkeyTests.cs: checks how a hotkey is shown to the user and when two hotkeys count as the same.

using WinIsland.Core.Hotkeys;

namespace WinIsland.Core.Tests.Hotkeys;

public class HotkeyTests
{
    [Fact]
    public void Text_lists_modifiers_in_a_fixed_order_then_the_key_in_upper_case()
    {
        Hotkey hotkey = new(HotkeyModifiers.Win | HotkeyModifiers.Alt | HotkeyModifiers.Shift | HotkeyModifiers.Ctrl, 'k');

        Assert.Equal("Ctrl+Alt+Shift+Win+K", hotkey.ToString());
    }

    [Fact]
    public void Same_combination_ignores_letter_case()
    {
        Hotkey upper = new(HotkeyModifiers.Alt, 'X');
        Hotkey lower = new(HotkeyModifiers.Alt, 'x');

        Assert.True(upper.IsSameCombinationAs(lower));
    }

    [Fact]
    public void Different_modifiers_are_different_combinations()
    {
        Hotkey alt = new(HotkeyModifiers.Alt, 'X');
        Hotkey ctrlAlt = new(HotkeyModifiers.Ctrl | HotkeyModifiers.Alt, 'X');

        Assert.False(alt.IsSameCombinationAs(ctrlAlt));
    }

    [Theory]
    [InlineData('A', true)]
    [InlineData('z', true)]
    [InlineData('7', true)]
    [InlineData(' ', false)]
    [InlineData('-', false)]
    [InlineData('é', false)]
    public void Only_letters_and_digits_are_valid_keys(char key, bool expected)
    {
        Assert.Equal(expected, new Hotkey(HotkeyModifiers.Alt, key).HasValidKey);
    }
}
