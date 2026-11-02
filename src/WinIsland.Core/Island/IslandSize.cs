// IslandSize.cs: the island size chosen in Settings (PRD FR-32). Medium is today's island and the
// default; the others make everything in the island smaller or bigger by the same factor
// (IslandScale.FactorOf).

namespace WinIsland.Core.Island;

public enum IslandSize
{
    Small,
    Medium,
    Large,
    ExtraLarge,
}
