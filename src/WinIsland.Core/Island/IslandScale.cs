// IslandScale.cs: works out how much bigger or smaller to draw the island on the current display.
// PRD FR-1: the island is designed for a 2560 DIP wide display and scales with the display
// width, limited to 0.85 and 1.25, so it suits 24 to 32 inch monitors.
// PRD FR-32 (v1.3): the size setting multiplies that scale: Small 0.85, Medium 1, Large 1.15,
// Extra large 1.3 (DESIGN_SPEC section 10).

namespace WinIsland.Core.Island;

public static class IslandScale
{
    public const double DesignWidthDips = 2560;
    public const double Minimum = 0.85;
    public const double Maximum = 1.25;

    /// <summary>How much the size setting makes the island smaller or bigger than Medium (FR-32).</summary>
    public static double FactorOf(IslandSize size)
    {
        return size switch
        {
            IslandSize.Small => 0.85,
            IslandSize.Medium => 1.0,
            IslandSize.Large => 1.15,
            IslandSize.ExtraLarge => 1.3,
            _ => throw new ArgumentOutOfRangeException(nameof(size), size, "Unknown island size."),
        };
    }

    /// <summary>The display's automatic scale (FR-1) times the size setting (FR-32).</summary>
    public static double ForDisplayWidth(double displayWidthDips, IslandSize size)
    {
        return ForDisplayWidth(displayWidthDips) * FactorOf(size);
    }

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
