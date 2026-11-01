// ColorMath.cs: the color arithmetic the artwork colors need (PRD FR-25, DESIGN_SPEC section 9):
// contrast between two colors, a see-through color laid over another, and the conversion between
// red-green-blue and hue-saturation-lightness. Contrast uses the WCAG 2 formula, the usual
// readability rule (4.5 to 1 for small text, 3 to 1 for controls).

namespace WinIsland.Core.Music;

public static class ColorMath
{
    public static double Contrast(RgbColor first, RgbColor second)
    {
        double a = Luminance(first);
        double b = Luminance(second);
        return (Math.Max(a, b) + 0.05) / (Math.Min(a, b) + 0.05);
    }

    /// <summary>The color seen when the top color, at this opacity (0 to 1), lies over the bottom one.</summary>
    public static RgbColor Over(RgbColor top, double opacity, RgbColor bottom)
    {
        return new RgbColor(
            Blend(top.R, bottom.R, opacity),
            Blend(top.G, bottom.G, opacity),
            Blend(top.B, bottom.B, opacity));
    }

    /// <summary>Hue in degrees (0 to 360), saturation and lightness from 0 to 1.</summary>
    public static (double Hue, double Saturation, double Lightness) ToHsl(RgbColor color)
    {
        double r = color.R / 255.0;
        double g = color.G / 255.0;
        double b = color.B / 255.0;
        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        double lightness = (max + min) / 2;
        if (max == min)
        {
            return (0, 0, lightness);
        }

        double range = max - min;
        double saturation = lightness > 0.5 ? range / (2 - max - min) : range / (max + min);
        double hue = max == r ? ((g - b) / range) + (g < b ? 6 : 0)
            : max == g ? ((b - r) / range) + 2
            : ((r - g) / range) + 4;
        return (hue * 60, saturation, lightness);
    }

    public static RgbColor FromHsl(double hue, double saturation, double lightness)
    {
        double amount = saturation * Math.Min(lightness, 1 - lightness);
        return new RgbColor(Channel(0), Channel(8), Channel(4));

        byte Channel(double offset)
        {
            double k = (offset + (hue / 30)) % 12;
            double value = lightness - (amount * Math.Max(-1, Math.Min(k - 3, Math.Min(9 - k, 1))));
            return (byte)Math.Round(Math.Clamp(value, 0, 1) * 255);
        }
    }

    private static double Luminance(RgbColor color)
    {
        return (0.2126 * Linear(color.R)) + (0.7152 * Linear(color.G)) + (0.0722 * Linear(color.B));
    }

    private static double Linear(byte channel)
    {
        double value = channel / 255.0;
        return value <= 0.03928 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
    }

    private static byte Blend(byte top, byte bottom, double opacity)
    {
        return (byte)Math.Round((top * opacity) + (bottom * (1 - opacity)));
    }
}
