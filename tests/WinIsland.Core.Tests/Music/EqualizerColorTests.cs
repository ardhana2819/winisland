// EqualizerColorTests.cs: checks the compact pill's equalizer color (FR-48, AC-49): the artwork's
// accent with artwork, white for grey artwork, and green without artwork.

using WinIsland.Core.Music;

namespace WinIsland.Core.Tests.Music;

public class EqualizerColorTests
{
    [Fact]
    public void A_song_with_artwork_gets_the_artwork_accent()
    {
        ArtworkPalette palette = new(new RgbColor(0x3F, 0x12, 0x16), new RgbColor(0xE8, 0x54, 0x61), RgbColor.White, IsNeutral: false);

        Assert.Equal(new RgbColor(0xE8, 0x54, 0x61), EqualizerColor.ForPill(palette, hasArtwork: true));
    }

    [Fact]
    public void Grey_artwork_gives_white_bars()
    {
        const int Pixels = 64 * 64;
        byte[] grey = new byte[Pixels * 4];
        for (int index = 0; index < Pixels; index++)
        {
            grey[(index * 4) + 0] = 0x80;
            grey[(index * 4) + 1] = 0x80;
            grey[(index * 4) + 2] = 0x80;
            grey[(index * 4) + 3] = 0xFF;
        }

        Assert.Equal(RgbColor.White, EqualizerColor.ForPill(ArtworkPalette.FromBgra32(grey), hasArtwork: true));
    }

    [Fact]
    public void A_song_without_artwork_keeps_green_bars()
    {
        Assert.Equal(new RgbColor(0x4A, 0xDE, 0x80), EqualizerColor.ForPill(ArtworkPalette.Black, hasArtwork: false));
    }

    [Fact]
    public void A_missing_palette_is_refused()
    {
        Assert.Throws<ArgumentNullException>(() => EqualizerColor.ForPill(null!, hasArtwork: true));
    }
}
