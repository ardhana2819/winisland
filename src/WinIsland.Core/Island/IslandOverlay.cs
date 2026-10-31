// IslandOverlay.cs: what the island has opened on top of its resting view (clock or music).
// Like the prototype, only one can be open, and the latest request wins: a notification banner
// replaces the expanded island (PRD FR-2), expanding replaces a banner, and tapping a banner
// opens the list.

namespace WinIsland.Core.Island;

public enum IslandOverlay
{
    None,
    Expanded,
    Banner,
    List,
}
