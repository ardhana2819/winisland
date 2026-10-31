// IslandView.cs: which shape the island is showing when it is visible.
// More views (notification banner, list, permission prompt) are added in Phase 4.

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
}
