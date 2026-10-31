// TrayMenuLabels.cs: the words on the tray menu, which change with the island's state (PRD FR-6).
// Wording follows the design board: "Pause for 1 hour", "Hide island (Ctrl+Alt+H)", "Quit".
// While paused, Pause becomes Resume; while hidden, Hide becomes Show.

using WinIsland.Core.Hotkeys;

namespace WinIsland.Core.Tray;

public static class TrayMenuLabels
{
    public const string Pause = "Pause for 1 hour";
    public const string Resume = "Resume";
    public const string Quit = "Quit";

    public static string PauseOrResume(bool isPaused) => isPaused ? Resume : Pause;

    public static string HideOrShow(bool isHidden, Hotkey hideShowHotkey)
    {
        ArgumentNullException.ThrowIfNull(hideShowHotkey);
        string verb = isHidden ? "Show" : "Hide";
        return $"{verb} island ({hideShowHotkey})";
    }
}
