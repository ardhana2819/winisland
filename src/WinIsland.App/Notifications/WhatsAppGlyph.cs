// WhatsAppGlyph.cs: the WhatsApp logo shape (speech bubble with a phone), drawn white on the
// green app circle (Diva, 1 Nov 2026). WinIsland draws it itself, so it shows for WhatsApp desktop
// and WhatsApp Web alike, also before Windows has sent any logo; the WhatsApp Web app from Chrome
// sends none at all.
// The shape is the WhatsApp icon from Simple Icons (CC0), on a 24 x 24 grid. Its arcs are written
// with spaced flags ("0 0 1"), because WPF reads "01" as one number.

using System.Windows;
using System.Windows.Media;

namespace WinIsland.App.Notifications;

internal static class WhatsAppGlyph
{
    /// <summary>The logo's size as a share of the circle's diameter (DESIGN_SPEC section 9).</summary>
    public const double ShareOfCircle = 0.58;

    private const double GridSize = 24;

    private const string LogoPath =
        "M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164" +
        "-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059" +
        "-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52" +
        "-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372" +
        "-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487" +
        ".709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413" +
        ".248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347" +
        "m-5.421 7.403h-.004a9.87 9.87 0 0 1 -5.031 -1.378l-.361-.214-3.741.982.998-3.648-.235-.374" +
        "a9.86 9.86 0 0 1 -1.51 -5.26c.001-5.45 4.436-9.884 9.888-9.884 2.64 0 5.122 1.03 6.988 2.898" +
        "a9.825 9.825 0 0 1 2.893 6.994c-.003 5.45-4.437 9.884-9.885 9.884" +
        "m8.413-18.297A11.815 11.815 0 0 0 12.05 0C5.495 0 .16 5.335.157 11.892c0 2.096.547 4.142 1.588 5.945" +
        "L.057 24l6.305-1.654a11.882 11.882 0 0 0 5.683 1.448h.005c6.554 0 11.89-5.335 11.893-11.893" +
        "a11.821 11.821 0 0 0 -3.48 -8.413Z";

    private static readonly Geometry Logo = Geometry.Parse(LogoPath);

    /// <summary>
    /// The logo sized for a circle of this diameter, moved so its ink starts at (0,0); a Path
    /// centered in the circle then puts the logo exactly in the middle.
    /// </summary>
    public static Geometry ForCircle(double diameter)
    {
        double scale = diameter * ShareOfCircle / GridSize;
        Rect ink = Logo.Bounds;
        TransformGroup placement = new();
        placement.Children.Add(new TranslateTransform(-ink.X, -ink.Y));
        placement.Children.Add(new ScaleTransform(scale, scale));

        Geometry sized = Logo.Clone();
        sized.Transform = placement;
        sized.Freeze();
        return sized;
    }
}
