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

    /// <summary>Space while expanded: play or pause (see ExpandedShortcut.cs).</summary>
    MusicPlayPause,

    /// <summary>Ctrl+Right while expanded: next song.</summary>
    MusicNext,

    /// <summary>Ctrl+Left while expanded: previous song.</summary>
    MusicPrevious,

    /// <summary>Alt+Down while expanded: the notifications page (FR-21).</summary>
    ShowNotificationsPage,

    /// <summary>Alt+Up while expanded: the music page (FR-21).</summary>
    ShowMusicPage,

    /// <summary>Alt+Left while a chat is open on the notifications page: back to the list (FR-35).</summary>
    CloseChat,
}
