// IslandView.cs: which shape the island is showing when it is visible.
// More views (music, notification banner, list, permission prompt, hotkey conflict) are added
// in the phases that build them; Phase 2 only needs these two.

namespace WinIsland.Core.Island;

public enum IslandView
{
    /// <summary>The small pill: the clock (or, from Phase 3, the current song).</summary>
    Compact,

    /// <summary>The large island with music controls. Only possible while music is present.</summary>
    Expanded,
}
