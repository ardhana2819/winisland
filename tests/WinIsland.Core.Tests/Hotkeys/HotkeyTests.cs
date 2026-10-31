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
    [InlineData("Ctrl+Alt+I", HotkeyModifiers.Ctrl | HotkeyModifiers.Alt, 'I')]
    [InlineData("alt+x", HotkeyModifiers.Alt, 'X')]
    [InlineData(" Ctrl + Shift + 7 ", HotkeyModifiers.Ctrl | HotkeyModifiers.Shift, '7')]
    [InlineData("Win+Alt+Ctrl+Shift+K", HotkeyModifiers.Ctrl | HotkeyModifiers.Alt | HotkeyModifiers.Shift | HotkeyModifiers.Win, 'K')]
    public void Text_reads_back_into_a_hotkey(string text, HotkeyModifiers modifiers, char key)
    {
        Assert.Equal(new Hotkey(modifiers, key), Hotkey.TryParse(text));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Ctrl+")]
    [InlineData("Ctrl+Alt+F5")]
    [InlineData("Hyper+X")]
    [InlineData("Ctrl+-")]
    public void Text_that_is_not_a_hotkey_reads_as_nothing(string? text)
    {
        Assert.Null(Hotkey.TryParse(text));
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
