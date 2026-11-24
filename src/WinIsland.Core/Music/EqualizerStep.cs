// EqualizerStep.cs: one move of one equalizer bar: grow or shrink to a height over a time
// (PRD FR-48, DESIGN_SPEC section 15).

namespace WinIsland.Core.Music;

/// <summary>Move the bar to <paramref name="HeightDips"/> over <paramref name="Seconds"/>.</summary>
public readonly record struct EqualizerStep(double HeightDips, double Seconds);
