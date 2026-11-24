// EqualizerColor.cs: picks the color of the equalizer bars in the compact music pill (PRD FR-48,
// DESIGN_SPEC section 15): the artwork's accent while the song has artwork, green as before when
// it has none. Grey artwork has a white accent already (FR-25), so it gives white bars.

namespace WinIsland.Core.Music;

public static class EqualizerColor
{
    /// <summary>The bars' color when the song has no artwork, as in v1.0 (#4ADE80).</summary>
    public static readonly RgbColor NoArtwork = new(0x4A, 0xDE, 0x80);

    public static RgbColor ForPill(ArtworkPalette palette, bool hasArtwork)
    {
        ArgumentNullException.ThrowIfNull(palette);
        return hasArtwork ? palette.Accent : NoArtwork;
    }
}
