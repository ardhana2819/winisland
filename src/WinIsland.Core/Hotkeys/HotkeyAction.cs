// HotkeyAction.cs: what each of WinIsland's hotkeys does (PRD FR-5).

namespace WinIsland.Core.Hotkeys;

public enum HotkeyAction
{
    /// <summary>Hotkey 1: expand or collapse the island.</summary>
    Expand,

    /// <summary>Hotkey 2: hide or show the island.</summary>
    HideShow,

    /// <summary>Hotkey 3: hide the notification banner. Registered once banners exist (Phase 4).</summary>
    HideBanner,
}
