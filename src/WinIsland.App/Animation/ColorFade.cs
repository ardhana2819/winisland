// ColorFade.cs: fades a brush, or one color stop of a gradient, to a new color over 0.45 s
// (DESIGN_SPEC section 9: the artwork colors fade when the song changes, and between the music page
// and black; section 13: the glass music page's gradient fades the same way). With Windows
// animation effects off the color changes at once.

using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WinIsland.App.Animation;

internal static class ColorFade
{
    private const double FadeSeconds = 0.45;

    private static readonly IEasingFunction EaseCurve = CubicBezierEase.Ease();

    public static void To(SolidColorBrush brush, Color color)
    {
        ArgumentNullException.ThrowIfNull(brush);
        Run(brush, SolidColorBrush.ColorProperty, color);
    }

    public static void To(GradientStop stop, Color color)
    {
        ArgumentNullException.ThrowIfNull(stop);
        Run(stop, GradientStop.ColorProperty, color);
    }

    private static void Run(Animatable target, System.Windows.DependencyProperty colorProperty, Color color)
    {
        if (!Motion.IsEnabled)
        {
            target.BeginAnimation(colorProperty, null);
            target.SetValue(colorProperty, color);
            return;
        }

        ColorAnimation fade = new(color, TimeSpan.FromSeconds(FadeSeconds)) { EasingFunction = EaseCurve };
        fade.Freeze();
        target.BeginAnimation(colorProperty, fade);
    }
}
