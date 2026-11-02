// IslandLayout.cs: works out where the island window goes and how big it is.
// The window has a fixed size, big enough for the largest island shape plus room for the spring
// overshoot, so it never has to resize while the island animates (resizing a transparent window
// every frame is slow). Clicks on the clear parts of the window pass through to the game.
// The size setting (FR-32) scales the window and everything in it together with the display scale.

using WinIsland.Core.Island;

namespace WinIsland.App.Island;

internal readonly record struct IslandLayout(double Scale, double Left, double Top, double Width, double Height)
{
    // Largest island in DESIGN_SPEC section 2 is 420 x 190 (notification list) plus 12 from the top.
    // The extra room leaves space for the spring overshoot (DESIGN_SPEC section 6).
    public const double CanvasWidthDips = 480;
    public const double CanvasHeightDips = 230;

    public static IslandLayout ForPrimaryDisplay(double displayWidthDips, IslandSize size)
    {
        double scale = IslandScale.ForDisplayWidth(displayWidthDips, size);
        double width = CanvasWidthDips * scale;
        double height = CanvasHeightDips * scale;
        double left = (displayWidthDips - width) / 2;
        return new IslandLayout(scale, left, 0, width, height);
    }
}
