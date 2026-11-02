// AppLogoGlyph.cs: the WhatsApp and Discord logo shapes, drawn white on their app circle
// (Diva, 1 Nov 2026 for WhatsApp; PRD FR-30, 2 Nov 2026 for Discord). WinIsland draws them
// itself, so they look the same whatever Windows sends, also before Windows has sent any logo;
// the WhatsApp Web app from Chrome sends none at all.
// Both shapes are the icons from Simple Icons (CC0), on a 24 x 24 grid. Their arcs are written
// with spaced flags ("0 0 1"), because WPF reads "01" as one number.

using System.Windows;
using System.Windows.Media;

using WinIsland.Core.Notifications;

namespace WinIsland.App.Notifications;

internal static class AppLogoGlyph
{
    /// <summary>The logo's size as a share of the circle's diameter (DESIGN_SPEC sections 9 and 10).</summary>
    public const double ShareOfCircle = 0.58;

    private const double GridSize = 24;

    private const string WhatsAppPath =
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

    // Every number is separated by a space here: the source writes ".0741.0741" and "00" in arcs.
    private const string DiscordPath =
        "M 20.317 4.3698 a 19.7913 19.7913 0 0 0 -4.8851 -1.5152 .0741 .0741 0 0 0 -.0785 .0371 c -.211 .3753 " +
        "-.4447 .8648 -.6083 1.2495 -1.8447 -.2762 -3.68 -.2762 -5.4868 0 -.1636 -.3933 -.4058 -.8742 -.6177 " +
        "-1.2495 a .077 .077 0 0 0 -.0785 -.037 19.7363 19.7363 0 0 0 -4.8852 1.515 .0699 .0699 0 0 0 -.0321 " +
        ".0277 C .5334 9.0458 -.319 13.5799 .0992 18.0578 a .0824 .0824 0 0 0 .0312 .0561 c 2.0528 1.5076 " +
        "4.0413 2.4228 5.9929 3.0294 a .0777 .0777 0 0 0 .0842 -.0276 c .4616 -.6304 .8731 -1.2952 1.226 " +
        "-1.9942 a .076 .076 0 0 0 -.0416 -.1057 c -.6528 -.2476 -1.2743 -.5495 -1.8722 -.8923 a .077 .077 0 " +
        "0 1 -.0076 -.1277 c .1258 -.0943 .2517 -.1923 .3718 -.2914 a .0743 .0743 0 0 1 .0776 -.0105 c 3.9278 " +
        "1.7933 8.18 1.7933 12.0614 0 a .0739 .0739 0 0 1 .0785 .0095 c .1202 .099 .246 .1981 .3728 .2924 a " +
        ".077 .077 0 0 1 -.0066 .1276 12.2986 12.2986 0 0 1 -1.873 .8914 .0766 .0766 0 0 0 -.0407 .1067 c " +
        ".3604 .698 .7719 1.3628 1.225 1.9932 a .076 .076 0 0 0 .0842 .0286 c 1.961 -.6067 3.9495 -1.5219 " +
        "6.0023 -3.0294 a .077 .077 0 0 0 .0313 -.0552 c .5004 -5.177 -.8382 -9.6739 -3.5485 -13.6604 a .061 " +
        ".061 0 0 0 -.0312 -.0286 z M 8.02 15.3312 c -1.1825 0 -2.1569 -1.0857 -2.1569 -2.419 0 -1.3332 .9555 " +
        "-2.4189 2.157 -2.4189 1.2108 0 2.1757 1.0952 2.1568 2.419 0 1.3332 -.9555 2.4189 -2.1569 2.4189 z m " +
        "7.9748 0 c -1.1825 0 -2.1569 -1.0857 -2.1569 -2.419 0 -1.3332 .9554 -2.4189 2.1569 -2.4189 1.2108 0 " +
        "2.1757 1.0952 2.1568 2.419 0 1.3332 -.946 2.4189 -2.1568 2.4189 Z";

    private static readonly Geometry WhatsApp = Geometry.Parse(WhatsAppPath);
    private static readonly Geometry Discord = Geometry.Parse(DiscordPath);

    /// <summary>
    /// The app's logo sized for a circle of this diameter, moved so its ink starts at (0,0); a Path
    /// centered in the circle then puts the logo exactly in the middle. Null for apps without a drawn logo.
    /// </summary>
    public static Geometry? ForCircle(NotificationSource source, double diameter)
    {
        Geometry? logo = source switch
        {
            NotificationSource.WhatsApp => WhatsApp,
            NotificationSource.Discord => Discord,
            _ => null,
        };
        if (logo is null)
        {
            return null;
        }

        double scale = diameter * ShareOfCircle / GridSize;
        Rect ink = logo.Bounds;
        TransformGroup placement = new();
        placement.Children.Add(new TranslateTransform(-ink.X, -ink.Y));
        placement.Children.Add(new ScaleTransform(scale, scale));

        Geometry sized = logo.Clone();
        sized.Transform = placement;
        sized.Freeze();
        return sized;
    }
}
