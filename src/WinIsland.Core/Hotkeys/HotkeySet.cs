// HotkeySet.cs: WinIsland's three hotkeys and the rules they must follow.
// PRD FR-5 defaults: Ctrl+Alt+I expands, Ctrl+Alt+H hides or shows, Alt+X hides the banner.
// PRD section 12: two hotkeys cannot be the same. Every hotkey also needs a modifier, so a
// plain letter never stops working in games and other programs.

namespace WinIsland.Core.Hotkeys;

public sealed record HotkeySet(Hotkey Expand, Hotkey HideShow, Hotkey HideBanner)
{
    public static HotkeySet Defaults { get; } = new(
        new Hotkey(HotkeyModifiers.Ctrl | HotkeyModifiers.Alt, 'I'),
        new Hotkey(HotkeyModifiers.Ctrl | HotkeyModifiers.Alt, 'H'),
        new Hotkey(HotkeyModifiers.Alt, 'X'));

    /// <summary>Returns a plain-English problem for each broken rule. Empty means the set is valid.</summary>
    public IReadOnlyList<string> FindProblems()
    {
        List<string> problems = [];
        (string Name, Hotkey Hotkey)[] named =
        [
            ("Expand island", Expand),
            ("Hide or show island", HideShow),
            ("Hide notifications", HideBanner),
        ];

        foreach ((string name, Hotkey hotkey) in named)
        {
            if (!hotkey.HasModifier)
            {
                problems.Add($"{name}: add Ctrl, Alt, Shift or Win, so the key still works in other programs.");
            }

            if (!hotkey.HasValidKey)
            {
                problems.Add($"{name}: use a letter or a digit as the main key.");
            }
        }

        for (int first = 0; first < named.Length; first++)
        {
            for (int second = first + 1; second < named.Length; second++)
            {
                if (named[first].Hotkey.IsSameCombinationAs(named[second].Hotkey))
                {
                    problems.Add($"{named[first].Name} and {named[second].Name} both use {named[first].Hotkey}.");
                }
            }
        }

        return problems;
    }
}
