// AppLogoStyle.cs: how another app's logo is drawn on its round icon (PRD FR-42, DESIGN_SPEC
// section 12): like WhatsApp and Discord, the app's own colour as the circle and the logo in white
// on it (Diva, 3 Nov 2026: Claude orange with the white mark).
//   - A logo with see-through parts (more than a tenth of its pixels) is a shape: it is drawn white
//     on its main colour. A logo with no see-through parts is a full tile: it fills the circle as
//     it is.
//   - The main colour is found like the artwork colours (FR-25): every visible pixel votes for its
//     hue with its colourfulness squared, so the logo's strong colour wins over grey edges. The
//     colour is that hue at the winning pixels' own saturation and lightness, kept between 0.3 and
//     0.6 lightness so the white logo stays readable on it.
//   - A logo without colour (black, white or grey) gets the quiet grey circle #3A3A3C.

using WinIsland.Core.Music;

namespace WinIsland.Core.Notifications;

public sealed record AppLogoStyle(RgbColor Circle, bool IsShape)
{
    public static readonly RgbColor NeutralCircle = new(0x3A, 0x3A, 0x3C);

    private const int HueBuckets = 36;
    private const double BucketDegrees = 360.0 / HueBuckets;
    private const byte VisibleAlpha = 128;
    private const double ShapeSeeThroughShare = 0.1;
    private const double NeutralColorfulness = 0.06;
    private const double MinLightness = 0.3;
    private const double MaxLightness = 0.6;

    /// <summary>The style for a logo's pixels in BGRA order, 4 bytes each.</summary>
    public static AppLogoStyle FromBgra32(ReadOnlySpan<byte> pixels)
    {
        int total = pixels.Length / 4;
        if (total == 0)
        {
            return new AppLogoStyle(NeutralCircle, IsShape: false);
        }

        double[] votes = new double[HueBuckets];
        double[] hueSum = new double[HueBuckets];
        double[] saturationSum = new double[HueBuckets];
        double[] lightnessSum = new double[HueBuckets];
        int seeThrough = 0;
        int visible = 0;
        double colorfulnessSum = 0;

        for (int index = 0; index + 3 < pixels.Length; index += 4)
        {
            if (pixels[index + 3] < VisibleAlpha)
            {
                seeThrough++;
                continue;
            }

            visible++;
            (double hue, double saturation, double lightness) = ColorMath.ToHsl(new RgbColor(pixels[index + 2], pixels[index + 1], pixels[index]));
            double colorfulness = saturation * (1 - Math.Abs((2 * lightness) - 1));
            colorfulnessSum += colorfulness;
            double vote = colorfulness * colorfulness;
            int bucket = Math.Min(HueBuckets - 1, (int)(hue / BucketDegrees));
            votes[bucket] += vote;
            hueSum[bucket] += hue * vote;
            saturationSum[bucket] += saturation * vote;
            lightnessSum[bucket] += lightness * vote;
        }

        bool isShape = seeThrough > total * ShapeSeeThroughShare;
        if (visible == 0 || colorfulnessSum / visible < NeutralColorfulness)
        {
            return new AppLogoStyle(NeutralCircle, isShape);
        }

        int winner = Array.IndexOf(votes, votes.Max());
        double weight = votes[winner];
        RgbColor circle = ColorMath.FromHsl(
            hueSum[winner] / weight,
            saturationSum[winner] / weight,
            Math.Clamp(lightnessSum[winner] / weight, MinLightness, MaxLightness));
        return new AppLogoStyle(circle, isShape);
    }
}
