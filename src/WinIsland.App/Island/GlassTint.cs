// GlassTint.cs: the artwork color over the blur on the Glass music page (PRD FR-45, DESIGN_SPEC
// section 13, Diva 4 Nov 2026): 90 percent at the top, 50 percent at 45 percent of the height, none
// at the bottom, like the solid page fades to black but ending in plain glass. The color fades
// over 0.45 s when the song changes, and the whole tint fades in and out with the music page.

using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

using WinIsland.App.Animation;
using WinIsland.Core.Music;

namespace WinIsland.App.Island;

internal sealed class GlassTint
{
    private const double FadeSeconds = 0.45;
    private const byte TopAlpha = 230;
    private const byte MiddleAlpha = 128;

    private static readonly IEasingFunction EaseCurve = CubicBezierEase.Ease();

    private readonly UIElement _layer;
    private readonly GradientStop _top;
    private readonly GradientStop _middle;
    private readonly GradientStop _bottom;

    public GlassTint(UIElement layer, GradientStop top, GradientStop middle, GradientStop bottom)
    {
        _layer = layer ?? throw new ArgumentNullException(nameof(layer));
        _top = top ?? throw new ArgumentNullException(nameof(top));
        _middle = middle ?? throw new ArgumentNullException(nameof(middle));
        _bottom = bottom ?? throw new ArgumentNullException(nameof(bottom));
        _layer.Opacity = 0;
    }

    /// <summary>Fades the tint in with this artwork color, or out (the color is kept while it fades).</summary>
    public void Show(bool visible, RgbColor color)
    {
        if (visible)
        {
            ColorFade.To(_top, Color.FromArgb(TopAlpha, color.R, color.G, color.B));
            ColorFade.To(_middle, Color.FromArgb(MiddleAlpha, color.R, color.G, color.B));
            ColorFade.To(_bottom, Color.FromArgb(0, color.R, color.G, color.B));
        }

        Motion.Run(_layer, UIElement.OpacityProperty, Motion.To(visible ? 1 : 0, FadeSeconds, EaseCurve));
    }
}
