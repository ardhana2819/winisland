// ArtworkBrush.cs: paints album art, or the design's placeholder when there is none.
// DESIGN_SPEC section 9 (v1.1): the placeholder is a #2C2C2E square with a #8E8E93 music note at
// 45 percent of its size, used for the empty view and for songs without artwork. It replaces the
// v1.0 gradient.

using System.Windows;
using System.Windows.Media;

namespace WinIsland.App.Music;

internal static class ArtworkBrush
{
    // The prototype's note icon in a 24 x 24 box, like its SVG viewBox.
    private const string NoteIconPath = "M9 3v11.6A3.5 3.5 0 1 0 11 18V8h6V3z";
    private const double BoxSize = 24;
    private const double NoteScale = 0.45;

    private static readonly Brush Placeholder = CreatePlaceholder();

    public static Brush For(ImageSource? artwork)
    {
        if (artwork is null)
        {
            return Placeholder;
        }

        ImageBrush brush = new(artwork) { Stretch = Stretch.UniformToFill };
        brush.Freeze();
        return brush;
    }

    private static Brush CreatePlaceholder()
    {
        GeometryDrawing background = new(
            new SolidColorBrush(Color.FromRgb(0x2C, 0x2C, 0x2E)),
            null,
            new RectangleGeometry(new Rect(0, 0, BoxSize, BoxSize)));

        Geometry note = Geometry.Parse(NoteIconPath).Clone();
        note.Transform = new ScaleTransform(NoteScale, NoteScale, BoxSize / 2, BoxSize / 2);
        GeometryDrawing noteDrawing = new(new SolidColorBrush(Color.FromRgb(0x8E, 0x8E, 0x93)), null, note);

        DrawingGroup drawing = new();
        drawing.Children.Add(background);
        drawing.Children.Add(noteDrawing);

        DrawingBrush brush = new(drawing) { Stretch = Stretch.Fill };
        brush.Freeze();
        return brush;
    }
}
