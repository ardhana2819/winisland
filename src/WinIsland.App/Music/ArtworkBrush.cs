// ArtworkBrush.cs: paints album art, or the design's stand-in gradient when there is none.
// DESIGN_SPEC section 4: use the album art from the player; fallback is a 135 degree gradient
// from #7C3AED to #06B6D4.

using System.Windows;
using System.Windows.Media;

namespace WinIsland.App.Music;

internal static class ArtworkBrush
{
    private static readonly Brush Fallback = CreateFallback();

    public static Brush For(ImageSource? artwork)
    {
        if (artwork is null)
        {
            return Fallback;
        }

        ImageBrush brush = new(artwork) { Stretch = Stretch.UniformToFill };
        brush.Freeze();
        return brush;
    }

    private static Brush CreateFallback()
    {
        LinearGradientBrush gradient = new(
            Color.FromRgb(0x7C, 0x3A, 0xED),
            Color.FromRgb(0x06, 0xB6, 0xD4),
            new Point(0, 0),
            new Point(1, 1));
        gradient.Freeze();
        return gradient;
    }
}
