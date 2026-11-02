// IslandLook.cs: the island's look chosen in Settings (PRD FR-37, v1.4). Solid is the black island
// and the default; Glass shows Windows' own blur of what is behind the island.

namespace WinIsland.Core.Island;

public enum IslandLook
{
    Solid,
    Glass,
}
