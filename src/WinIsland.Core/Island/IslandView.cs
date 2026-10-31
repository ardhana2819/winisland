// IslandView.cs: which shape the island is showing when it is visible.
// More views (music, notification banner, list, permission prompt) are added in the phases
// that build them.

namespace WinIsland.Core.Island;

public enum IslandView
{
    /// <summary>The small pill: the clock (or, from Phase 3, the current song).</summary>
    Compact,

    /// <summary>The large island with music controls. Only possible while music is present.</summary>
    Expanded,

    /// <summary>The warning banner shown at start when a hotkey is taken by another app (FR-5).</summary>
    HotkeyConflict,
}
