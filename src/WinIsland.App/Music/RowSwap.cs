// RowSwap.cs: swaps the progress row and the volume row in their shared place (PRD FR-39,
// DESIGN_SPEC section 12). The same both ways, like the island's own content swap: the leaving
// row fades, blurs by 4 and shrinks to 0.96, the arriving row does the reverse, over 0.3 s on the
// ease curve. The blur is removed once the swap ends. With Windows animation effects off it is
// instant.

using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

using WinIsland.App.Animation;

namespace WinIsland.App.Music;

internal static class RowSwap
{
    private const double Seconds = 0.3;
    private const double HiddenScale = 0.96;
    private const double BlurDips = 4;

    /// <summary>Puts a row in its resting state at once: shown or hidden.</summary>
    public static void Set(FrameworkElement row, bool isShown)
    {
        ScaleTransform scale = ScaleOf(row);
        row.BeginAnimation(UIElement.OpacityProperty, null);
        scale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
        scale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
        row.Opacity = isShown ? 1 : 0;
        scale.ScaleX = isShown ? 1 : HiddenScale;
        scale.ScaleY = isShown ? 1 : HiddenScale;
        row.Effect = null;
        row.IsHitTestVisible = isShown;
    }

    public static void Swap(FrameworkElement leaving, FrameworkElement arriving)
    {
        if (!Motion.IsEnabled)
        {
            Set(leaving, false);
            Set(arriving, true);
            return;
        }

        leaving.IsHitTestVisible = false;
        arriving.IsHitTestVisible = true;
        Move(leaving, toShown: false);
        Move(arriving, toShown: true);
    }

    private static void Move(FrameworkElement row, bool toShown)
    {
        IEasingFunction ease = CubicBezierEase.Ease();
        ScaleTransform scale = ScaleOf(row);
        BlurEffect blur = row.Effect as BlurEffect ?? new BlurEffect { Radius = toShown ? BlurDips : 0 };
        row.Effect = blur;

        row.BeginAnimation(UIElement.OpacityProperty, Motion.To(toShown ? 1 : 0, Seconds, ease));
        scale.BeginAnimation(ScaleTransform.ScaleXProperty, Motion.To(toShown ? 1 : HiddenScale, Seconds, ease));
        scale.BeginAnimation(ScaleTransform.ScaleYProperty, Motion.To(toShown ? 1 : HiddenScale, Seconds, ease));
        DoubleAnimation blurMove = Motion.To(toShown ? 0 : BlurDips, Seconds, ease).Clone();
        blurMove.Completed += (_, _) =>
        {
            // Resting rows draw without a blur, which costs nothing (PRD G4).
            if (row.Effect == blur)
            {
                row.Effect = null;
            }
        };
        blur.BeginAnimation(BlurEffect.RadiusProperty, blurMove);
    }

    private static ScaleTransform ScaleOf(FrameworkElement row)
    {
        if (row.RenderTransform is ScaleTransform existing && !existing.IsFrozen)
        {
            return existing;
        }

        ScaleTransform scale = new(1, 1);
        row.RenderTransformOrigin = new Point(0.5, 0.5);
        row.RenderTransform = scale;
        return scale;
    }
}
