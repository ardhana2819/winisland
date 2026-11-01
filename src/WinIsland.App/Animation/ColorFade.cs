// ColorFade.cs: fades a brush to a new color over 0.45 s (DESIGN_SPEC section 9: the artwork colors
// fade when the song changes, and between the music page and black). With Windows animation
// effects off the color changes at once.

using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WinIsland.App.Animation;

internal static class ColorFade
{
    private const double FadeSeconds = 0.45;

    private static readonly IEasingFunction EaseCurve = CubicBezierEase.Ease();

    public static void To(SolidColorBrush brush, Color color)
    {
        if (!Motion.IsEnabled)
        {
            brush.BeginAnimation(SolidColorBrush.ColorProperty, null);
            brush.Color = color;
            return;
        }

        ColorAnimation fade = new(color, TimeSpan.FromSeconds(FadeSeconds)) { EasingFunction = EaseCurve };
        fade.Freeze();
        brush.BeginAnimation(SolidColorBrush.ColorProperty, fade);
    }
}
