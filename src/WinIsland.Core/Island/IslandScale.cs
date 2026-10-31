// IslandScale.cs: works out how much bigger or smaller to draw the island on the current display.
// PRD FR-1: the island is designed for a 2560 DIP wide display and scales with the display
// width, limited to 0.85 and 1.25, so it suits 24 to 32 inch monitors without a size setting.

namespace WinIsland.Core.Island;

public static class IslandScale
{
    public const double DesignWidthDips = 2560;
    public const double Minimum = 0.85;
    public const double Maximum = 1.25;

    public static double ForDisplayWidth(double displayWidthDips)
    {
        if (double.IsNaN(displayWidthDips) || double.IsInfinity(displayWidthDips) || displayWidthDips <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(displayWidthDips), displayWidthDips, "Display width must be a positive number of DIPs.");
        }

        double scale = displayWidthDips / DesignWidthDips;
        return Math.Clamp(scale, Minimum, Maximum);
    }
}
