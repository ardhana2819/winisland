// IslandOverlay.cs: what the island has opened on top of its resting view (clock or music).
// Like the prototype, only one can be open, and the latest request wins: a notification banner
// replaces the expanded island (PRD FR-2), and expanding replaces a banner. Since v1.1 the last 3
// notifications are a page of the expanded island (FR-21, FR-22), not an overlay of their own.

namespace WinIsland.Core.Island;

public enum IslandOverlay
{
    None,
    Expanded,
    Banner,
}
