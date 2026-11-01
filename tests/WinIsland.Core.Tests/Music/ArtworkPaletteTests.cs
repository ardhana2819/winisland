// ArtworkPaletteTests.cs: checks the music page colors from the artwork (FR-25, AC-14,
// DESIGN_SPEC section 9): the main color wins, black and white artwork gives a neutral grey, and
// text and controls stay readable for every color.

using WinIsland.Core.Music;

namespace WinIsland.Core.Tests.Music;

public class ArtworkPaletteTests
{
    private const int Pixels = 64 * 64;

    [Fact]
    public void A_red_cover_gives_a_dark_red_background_and_a_red_accent()
    {
        ArtworkPalette palette = ArtworkPalette.FromBgra32(Cover((new RgbColor(0x14, 0x04, 0x06), 0.7), (new RgbColor(0xE1, 0x1D, 0x2E), 0.3)));

        Assert.False(palette.IsNeutral);
        Assert.True(IsRed(palette.Background), palette.Background.ToString());
        Assert.True(IsRed(palette.Accent), palette.Accent.ToString());
    }

    [Fact]
    public void A_small_pink_shape_on_a_large_cream_area_gives_pink_not_brown()
    {
        // The prototype review: cream counted too much at first and the result was brown.
        ArtworkPalette palette = ArtworkPalette.FromBgra32(Cover((new RgbColor(0xF7, 0xF1, 0xE6), 0.72), (new RgbColor(0xF2, 0xB8, 0xC6), 0.28)));

        double hue = ColorMath.ToHsl(palette.Accent).Hue;
        Assert.InRange(hue, 320, 360);
    }

    [Fact]
    public void A_blue_cover_gives_blue()
    {
        ArtworkPalette palette = ArtworkPalette.FromBgra32(Cover((new RgbColor(0x0B, 0x1D, 0x4F), 0.5), (new RgbColor(0x1E, 0x88, 0xE5), 0.5)));

        double hue = ColorMath.ToHsl(palette.Accent).Hue;
        Assert.InRange(hue, 200, 240);
    }

    [Fact]
    public void Black_and_white_artwork_gives_a_neutral_grey_and_a_white_accent()
    {
        ArtworkPalette palette = ArtworkPalette.FromBgra32(Cover(
            (new RgbColor(0x0A, 0x0A, 0x0A), 0.25),
            (new RgbColor(0xE8, 0xE8, 0xE8), 0.25),
            (new RgbColor(0x6B, 0x6B, 0x6B), 0.25),
            (new RgbColor(0x1E, 0x1E, 0x1E), 0.25)));

        Assert.True(palette.IsNeutral);
        Assert.Equal(RgbColor.White, palette.Accent);
        (double _, double saturation, double _) = ColorMath.ToHsl(palette.Background);
        Assert.True(saturation < 0.1, palette.Background.ToString());
    }

    [Fact]
    public void Text_and_controls_stay_readable_for_every_color()
    {
        // AC-14: title, grey text and controls readable on any artwork, including very light and grey.
        foreach (int hue in Enumerable.Range(0, 36).Select(step => step * 10))
        {
            foreach (double saturation in new[] { 0.0, 0.2, 0.5, 1.0 })
            {
                foreach (double lightness in new[] { 0.05, 0.3, 0.5, 0.7, 0.95 })
                {
                    RgbColor color = ColorMath.FromHsl(hue, saturation, lightness);
                    ArtworkPalette palette = ArtworkPalette.FromBgra32(Cover((color, 1.0)));
                    string label = $"{color} gave {palette}";

                    Assert.True(ColorMath.Contrast(RgbColor.White, palette.Background) >= 4.5, label);
                    Assert.True(ColorMath.Contrast(palette.Secondary, palette.Background) >= 4.5, label);
                    Assert.True(ColorMath.Contrast(palette.Accent, palette.Background) >= 3, label);
                }
            }
        }
    }

    [Fact]
    public void No_pixels_or_see_through_pixels_give_the_black_look()
    {
        Assert.Equal(ArtworkPalette.Black, ArtworkPalette.FromBgra32([]));
        Assert.Equal(ArtworkPalette.Black, ArtworkPalette.FromBgra32(new byte[] { 0, 0, 255, 0 }));
        Assert.Equal(ArtworkPalette.Black, ArtworkPalette.FromBgra32(new byte[] { 1, 2, 3 }));
    }

    [Fact]
    public void Color_math_matches_the_known_values()
    {
        Assert.Equal(21, ColorMath.Contrast(RgbColor.Black, RgbColor.White), 3);
        Assert.Equal(new RgbColor(173, 173, 173), ColorMath.Over(RgbColor.White, 0.68, RgbColor.Black));
        Assert.Equal(new RgbColor(100, 0, 0), ColorMath.Over(new RgbColor(200, 0, 0), 0.5, RgbColor.Black));
        Assert.Equal(new RgbColor(255, 0, 0), ColorMath.FromHsl(0, 1, 0.5));
        Assert.Equal(new RgbColor(0, 0, 255), ColorMath.FromHsl(240, 1, 0.5));
        (double hue, double saturation, double lightness) = ColorMath.ToHsl(new RgbColor(0, 255, 0));
        Assert.Equal(120, hue, 3);
        Assert.Equal(1, saturation, 3);
        Assert.Equal(0.5, lightness, 3);
    }

    private static bool IsRed(RgbColor color) => color.R > color.G * 2 && color.R > color.B * 2;

    /// <summary>A 64 x 64 cover made of blocks of solid color, each taking its share of the pixels.</summary>
    private static byte[] Cover(params (RgbColor Color, double Share)[] parts)
    {
        byte[] pixels = new byte[Pixels * 4];
        int pixel = 0;
        foreach ((RgbColor color, double share) in parts)
        {
            int count = (int)Math.Round(share * Pixels);
            for (int index = 0; index < count && pixel < Pixels; index++, pixel++)
            {
                pixels[(pixel * 4) + 0] = color.B;
                pixels[(pixel * 4) + 1] = color.G;
                pixels[(pixel * 4) + 2] = color.R;
                pixels[(pixel * 4) + 3] = 255;
            }
        }

        return pixels;
    }
}
