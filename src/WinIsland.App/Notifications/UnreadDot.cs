// UnreadDot.cs: the small white dot on the idle island that says "a notification you have not
// seen yet" (PRD FR-33, DESIGN_SPEC section 10). It is 6 x 6; it pops in over 0.35 s (scale 0.4
// to 1 with the shape spring, opacity over 0.25 s) and fades out over 0.25 s. With Windows
// animation effects off it appears and disappears at once.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

using WinIsland.App.Animation;

namespace WinIsland.App.Notifications;

internal sealed class UnreadDot : Border
{
    private const double SizeDips = 6;
    private const double HiddenScale = 0.4;
    private const double PopSeconds = 0.35;
    private const double FadeSeconds = 0.25;

    private static readonly IEasingFunction SpringCurve = CubicBezierEase.Spring();
    private static readonly IEasingFunction EaseCurve = CubicBezierEase.Ease();

    private readonly ScaleTransform _scale = new(HiddenScale, HiddenScale);
    private bool _isShown;

    public UnreadDot()
    {
        Width = SizeDips;
        Height = SizeDips;
        CornerRadius = new CornerRadius(SizeDips / 2);
        Background = Brushes.White;
        Opacity = 0;
        IsHitTestVisible = false;
        Focusable = false;
        RenderTransformOrigin = new Point(0.5, 0.5);
        RenderTransform = _scale;
    }

    public void SetShown(bool shown)
    {
        if (shown == _isShown)
        {
            return;
        }

        _isShown = shown;
        Motion.Run(this, OpacityProperty, Motion.To(shown ? 1 : 0, FadeSeconds, EaseCurve));
        if (shown)
        {
            // Each pop starts small again.
            PopFromSmall(ScaleTransform.ScaleXProperty);
            PopFromSmall(ScaleTransform.ScaleYProperty);
        }
    }

    private void PopFromSmall(DependencyProperty property)
    {
        _scale.BeginAnimation(property, null);
        _scale.SetValue(property, HiddenScale);
        Motion.Run(_scale, property, Motion.To(1, PopSeconds, SpringCurve));
    }
}
