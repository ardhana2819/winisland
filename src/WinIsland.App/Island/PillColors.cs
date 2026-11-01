// PillColors.cs: colors the island behind the open music page (PRD FR-25, DESIGN_SPEC section 9).
// The background takes the artwork's dark color, and a shade over it fades to black towards the
// bottom (Diva, 1 Nov 2026): black at 0 percent at the top, 60 percent at 60 percent of the
// height, 100 percent at the bottom. Everywhere else (compact pill, banner, notifications page)
// the island is black. Changes fade over 0.45 s; the shade only darkens, so text stays readable.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

using WinIsland.App.Animation;
using WinIsland.Core.Music;

namespace WinIsland.App.Island;

internal sealed class PillColors
{
    private const double FadeSeconds = 0.45;

    private static readonly IEasingFunction EaseCurve = CubicBezierEase.Ease();

    private readonly SolidColorBrush _background = new(Colors.Black);
    private readonly UIElement _shade;
    private bool _isTinted;
    private RgbColor _shownColor = RgbColor.Black;

    public PillColors(Border pill, UIElement shade)
    {
        ArgumentNullException.ThrowIfNull(pill);
        ArgumentNullException.ThrowIfNull(shade);
        pill.Background = _background;
        _shade = shade;
        _shade.Opacity = 0;
    }

    /// <summary>
    /// tinted is true while the open island shows the music page with artwork colors; the color
    /// is the palette's background.
    /// </summary>
    public void Show(bool tinted, ArtworkPalette palette)
    {
        ArgumentNullException.ThrowIfNull(palette);
        RgbColor color = tinted ? palette.Background : RgbColor.Black;
        if (tinted == _isTinted && color == _shownColor)
        {
            return;
        }

        _isTinted = tinted;
        _shownColor = color;
        ColorFade.To(_background, Color.FromRgb(color.R, color.G, color.B));
        Motion.Run(_shade, UIElement.OpacityProperty, Motion.To(tinted ? 1 : 0, FadeSeconds, EaseCurve));
    }
}
