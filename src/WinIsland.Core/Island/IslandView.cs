// IslandView.cs: which shape the island is showing when it is visible.

namespace WinIsland.Core.Island;

public enum IslandView
{
    /// <summary>The small pill with the clock.</summary>
    Compact,

    /// <summary>The compact music pill: artwork, song title, equalizer (DESIGN_SPEC section 5).</summary>
    Music,

    /// <summary>The large island with music controls. Only possible while music is present.</summary>
    Expanded,

    /// <summary>The warning banner shown at start when a hotkey is taken by another app (FR-5).</summary>
    HotkeyConflict,

    /// <summary>A notification banner: app, sender, preview, +N badge (FR-15).</summary>
    Notification,

    /// <summary>The last 3 notifications, or the full text of a single one (FR-15).</summary>
    NotificationList,

    /// <summary>"Allow notification access", shown when Windows did not give access (FR-12, section 13).</summary>
    PermissionPrompt,
}
