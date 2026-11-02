// IslandLookResources.cs: the colors inside the island that change with the look (PRD FR-37,
// DESIGN_SPEC section 11). Views refer to these by name (DynamicResource in XAML,
// SetResourceReference in code), so switching the look recolors everything at once.
//   Solid: cards #1C1C1E, second lines #A6ABB6, badge #3A3A3C, peeking cards #3A3A3C and #2E2E31,
//          chat rows' text #E5E5EA.
//   Glass: cards white at 14 percent, second lines white at 85 percent (grey disappears over
//          bright games), badge white at 20 percent, peeking cards white at 10 and 6 percent,
//          chat rows' text white at 85 percent. Text also gets a small soft shadow.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

using WinIsland.Core.Island;

namespace WinIsland.App.Island;

internal static class IslandLookResources
{
    public const string CardBrush = "IslandCardBrush";
    public const string SecondaryTextBrush = "IslandSecondaryTextBrush";
    public const string BadgeBrush = "IslandBadgeBrush";
    public const string FirstBehindBrush = "IslandFirstBehindBrush";
    public const string SecondBehindBrush = "IslandSecondBehindBrush";
    public const string MessageTextBrush = "IslandMessageTextBrush";

    // Glass: black, blur radius 3, 1 down, 45 percent (DESIGN_SPEC section 11).
    private static readonly Style GlassTextStyle = CreateGlassTextStyle();

    /// <summary>Sets the colors for a look; textScope gets the text shadow on Glass.</summary>
    public static void Apply(ResourceDictionary resources, IslandLook look, FrameworkElement textScope)
    {
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(textScope);

        bool glass = look == IslandLook.Glass;
        resources[CardBrush] = Brush(glass ? Color.FromArgb(0x24, 0xFF, 0xFF, 0xFF) : Color.FromRgb(0x1C, 0x1C, 0x1E));
        resources[SecondaryTextBrush] = Brush(glass ? Color.FromArgb(0xD9, 0xFF, 0xFF, 0xFF) : Color.FromRgb(0xA6, 0xAB, 0xB6));
        resources[BadgeBrush] = Brush(glass ? Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF) : Color.FromRgb(0x3A, 0x3A, 0x3C));
        resources[FirstBehindBrush] = Brush(glass ? Color.FromArgb(0x1A, 0xFF, 0xFF, 0xFF) : Color.FromRgb(0x3A, 0x3A, 0x3C));
        resources[SecondBehindBrush] = Brush(glass ? Color.FromArgb(0x0F, 0xFF, 0xFF, 0xFF) : Color.FromRgb(0x2E, 0x2E, 0x31));
        resources[MessageTextBrush] = Brush(glass ? Color.FromArgb(0xD9, 0xFF, 0xFF, 0xFF) : Color.FromRgb(0xE5, 0xE5, 0xEA));

        // An implicit style reaches every TextBlock inside the island, also ones built in code.
        if (glass)
        {
            textScope.Resources[typeof(TextBlock)] = GlassTextStyle;
        }
        else
        {
            textScope.Resources.Remove(typeof(TextBlock));
        }
    }

    private static Style CreateGlassTextStyle()
    {
        DropShadowEffect shadow = new() { Color = Colors.Black, BlurRadius = 3, ShadowDepth = 1, Direction = 270, Opacity = 0.45 };
        shadow.Freeze();
        Style style = new(typeof(TextBlock));
        style.Setters.Add(new Setter(UIElement.EffectProperty, shadow));
        style.Seal();
        return style;
    }

    private static SolidColorBrush Brush(Color color)
    {
        SolidColorBrush brush = new(color);
        brush.Freeze();
        return brush;
    }
}
