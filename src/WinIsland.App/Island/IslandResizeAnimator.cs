// IslandResizeAnimator.cs: makes a size change from Settings (PRD FR-32) grow or shrink the island
// smoothly, with the same 0.6 s spring as the shape change (DESIGN_SPEC section 6), instead of
// jumping (Diva, 2 Nov 2026).
// The island is drawn at its old size while a render scale on top of it moves from 1 to the
// new-to-old ratio; at the end the real size is set and the render scale goes back to 1. A render
// scale moves pixels only, so nothing is laid out again on every frame. With Windows animation
// effects off, the new size shows at once.

using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

using WinIsland.App.Animation;

namespace WinIsland.App.Island;

internal sealed class IslandResizeAnimator
{
    private const double ResizeSeconds = 0.6;

    private static readonly IEasingFunction SpringCurve = CubicBezierEase.Spring();

    private readonly ScaleTransform _growScale;
    private Action? _finish;

    public IslandResizeAnimator(ScaleTransform growScale)
    {
        _growScale = growScale ?? throw new ArgumentNullException(nameof(growScale));
    }

    /// <summary>
    /// Scales the island from one size to the other, then calls finish, which sets the real new
    /// size. The island window always has room for the largest size.
    /// </summary>
    public void Run(double fromScale, double toScale, Action finish)
    {
        ArgumentNullException.ThrowIfNull(finish);
        FinishNow();

        if (!Motion.IsEnabled || fromScale <= 0 || fromScale == toScale)
        {
            finish();
            return;
        }

        _finish = finish;
        DoubleAnimation grow = Motion.To(toScale / fromScale, ResizeSeconds, SpringCurve).Clone();
        grow.Completed += (_, _) => FinishNow();
        _growScale.BeginAnimation(ScaleTransform.ScaleXProperty, grow);
        _growScale.BeginAnimation(ScaleTransform.ScaleYProperty, grow);
    }

    /// <summary>Ends a running resize at once: the real new size is set and the render scale is reset.</summary>
    public void FinishNow()
    {
        Action? finish = _finish;
        _finish = null;
        _growScale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
        _growScale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
        _growScale.ScaleX = 1;
        _growScale.ScaleY = 1;
        finish?.Invoke();
    }
}
