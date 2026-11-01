// ArtworkPalette.cs: works out the music page's colors from the song's artwork (PRD FR-25,
// DESIGN_SPEC section 9), the same steps the approved prototype uses:
//   1. Each pixel votes for its hue (36 buckets of 10 degrees) with its colorfulness squared,
//      so a small strong color beats a large pale area, and black, white and grey count almost
//      nothing. Colorfulness = saturation x (1 - |2 x lightness - 1|).
//   2. Average colorfulness under 0.06 (black and white or grey artwork): neutral dark grey
//      background and a white accent.
//   3. Background: the winning hue, saturation at most 0.55, lightness 0.16, darkened in steps
//      of 0.01 until the grey text (white at 68 percent over it) reaches 4.5 to 1.
//   4. Accent: same hue, saturation between 0.6 and 0.9, lightness 0.62, lightened in steps of
//      0.02 (up to 0.9) until it reaches 3 to 1 against the background.
// No artwork, or the empty view, uses Black: the v1.0 look.

namespace WinIsland.Core.Music;

public sealed record ArtworkPalette(RgbColor Background, RgbColor Accent, RgbColor Secondary, bool IsNeutral)
{
    /// <summary>Grey text is white at this opacity over the background.</summary>
    public const double SecondaryOpacity = 0.68;

    /// <summary>The play button's circle is the accent at this opacity.</summary>
    public const double AccentSoftOpacity = 0.22;

    private const int HueBuckets = 36;
    private const double NeutralColorfulness = 0.06;
    private const double NeutralHue = 240;
    private const double NeutralSaturation = 0.04;
    private const double BackgroundMaxSaturation = 0.55;
    private const double BackgroundLightness = 0.16;
    private const double BackgroundStep = 0.01;
    private const double BackgroundMinLightness = 0.04;
    private const double AccentMinSaturation = 0.6;
    private const double AccentMaxSaturation = 0.9;
    private const double AccentLightness = 0.62;
    private const double AccentStep = 0.02;
    private const double AccentMaxLightness = 0.9;
    private const double TextContrast = 4.5;
    private const double ControlContrast = 3;

    /// <summary>The v1.0 look: black background, white controls, the design's grey text.</summary>
    public static ArtworkPalette Black { get; } = new(RgbColor.Black, RgbColor.White, new RgbColor(0xA6, 0xAB, 0xB6), IsNeutral: true);

    /// <summary>
    /// The palette for artwork given as 32-bit pixels in blue, green, red, alpha order (the usual
    /// Windows layout). Pixels that are mostly see-through are skipped. Returns Black when there
    /// is nothing to read.
    /// </summary>
    public static ArtworkPalette FromBgra32(ReadOnlySpan<byte> pixels)
    {
        if (pixels.Length < 4 || pixels.Length % 4 != 0)
        {
            return Black;
        }

        double[] votes = new double[HueBuckets];
        double[] saturationSum = new double[HueBuckets];
        double[] hueSum = new double[HueBuckets];
        double colorfulnessSum = 0;
        int counted = 0;

        for (int index = 0; index < pixels.Length; index += 4)
        {
            if (pixels[index + 3] < 128)
            {
                continue;
            }

            RgbColor pixel = new(pixels[index + 2], pixels[index + 1], pixels[index]);
            (double hue, double saturation, double lightness) = ColorMath.ToHsl(pixel);
            double colorfulness = saturation * (1 - Math.Abs((2 * lightness) - 1));
            double vote = colorfulness * colorfulness;
            int bucket = (int)(hue / 10) % HueBuckets;
            votes[bucket] += vote;
            saturationSum[bucket] += saturation * vote;
            hueSum[bucket] += hue * vote;
            colorfulnessSum += colorfulness;
            counted++;
        }

        if (counted == 0)
        {
            return Black;
        }

        int best = 0;
        for (int bucket = 1; bucket < HueBuckets; bucket++)
        {
            if (votes[bucket] > votes[best])
            {
                best = bucket;
            }
        }

        bool isNeutral = colorfulnessSum / counted < NeutralColorfulness || votes[best] == 0;
        double mainHue = isNeutral ? NeutralHue : hueSum[best] / votes[best];
        double mainSaturation = isNeutral ? NeutralSaturation : saturationSum[best] / votes[best];
        return Build(mainHue, mainSaturation, isNeutral);
    }

    private static ArtworkPalette Build(double hue, double saturation, bool isNeutral)
    {
        double backgroundSaturation = Math.Min(saturation, BackgroundMaxSaturation);
        double lightness = BackgroundLightness;
        RgbColor background = ColorMath.FromHsl(hue, backgroundSaturation, lightness);
        RgbColor secondary = ColorMath.Over(RgbColor.White, SecondaryOpacity, background);
        while (ColorMath.Contrast(secondary, background) < TextContrast && lightness > BackgroundMinLightness)
        {
            lightness -= BackgroundStep;
            background = ColorMath.FromHsl(hue, backgroundSaturation, lightness);
            secondary = ColorMath.Over(RgbColor.White, SecondaryOpacity, background);
        }

        RgbColor accent = RgbColor.White;
        if (!isNeutral)
        {
            double accentSaturation = Math.Clamp(saturation, AccentMinSaturation, AccentMaxSaturation);
            double accentLightness = AccentLightness;
            accent = ColorMath.FromHsl(hue, accentSaturation, accentLightness);
            while (ColorMath.Contrast(accent, background) < ControlContrast && accentLightness < AccentMaxLightness)
            {
                accentLightness += AccentStep;
                accent = ColorMath.FromHsl(hue, accentSaturation, accentLightness);
            }
        }

        return new ArtworkPalette(background, accent, secondary, isNeutral);
    }
}
