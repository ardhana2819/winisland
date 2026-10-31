// PillAnimator.cs: plays the island's motion from DESIGN_SPEC section 6.
//   Shape change: width, height and corner radius over 0.6 s on the spring curve.
//   Content swap: old content fades, shrinks to 0.88 and blurs by 10; new content follows
//                 0.14 s later, from blur 10 and scale 0.88 back to normal.
//   Hide and show: fade over 0.3 s, scale between 0.4 and 1 and move 14 up over 0.5 s.
// The blur is removed once a swap ends, so a resting island costs nothing extra to draw.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

using WinIsland.App.Animation;

namespace WinIsland.App.Island;

internal sealed class PillAnimator
{
    private const double ShapeSeconds = 0.6;
    private const double HideFadeSeconds = 0.3;
    private const double HideMoveSeconds = 0.5;
    private const double HiddenScale = 0.4;
    private const double HiddenOffsetDips = -14;
    private const double ContentHiddenScale = 0.88;
    private const double ContentBlurDips = 10;
    private const double ContentDelaySeconds = 0.14;

    private static readonly IEasingFunction SpringCurve = CubicBezierEase.Spring();
    private static readonly IEasingFunction EaseCurve = CubicBezierEase.Ease();

    private readonly Border _pill;
    private readonly ScaleTransform _pillScale;
    private readonly TranslateTransform _pillOffset;

    public PillAnimator(Border pill, ScaleTransform pillScale, TranslateTransform pillOffset)
    {
        _pill = pill;
        _pillScale = pillScale;
        _pillOffset = pillOffset;
        PillGeometry.Attach(pill);
    }

    /// <summary>Sets the shape without animation, for the first frame.</summary>
    public void SetShape(PillShape shape)
    {
        _pill.Width = shape.Width;
        _pill.Height = shape.Height;
        PillGeometry.SetRadius(_pill, shape.CornerRadius);
    }

    public void MorphTo(PillShape shape)
    {
        Motion.Run(_pill, FrameworkElement.WidthProperty, Motion.To(shape.Width, ShapeSeconds, SpringCurve));
        Motion.Run(_pill, FrameworkElement.HeightProperty, Motion.To(shape.Height, ShapeSeconds, SpringCurve));
        Motion.Run(_pill, PillGeometry.RadiusProperty, Motion.To(shape.CornerRadius, ShapeSeconds, SpringCurve));
    }

    public void SwapContent(FrameworkElement oldContent, FrameworkElement newContent)
    {
        oldContent.IsHitTestVisible = false;
        newContent.IsHitTestVisible = true;
        ScaleTransform oldScale = ContentScale(oldContent);
        ScaleTransform newScale = ContentScale(newContent);

        if (!Motion.IsEnabled)
        {
            ShowContentInstantly(oldContent, newContent);
            return;
        }

        BlurEffect oldBlur = new() { Radius = 0 };
        oldContent.Effect = oldBlur;
        oldContent.BeginAnimation(UIElement.OpacityProperty, Motion.To(0, 0.2, EaseCurve));
        oldBlur.BeginAnimation(BlurEffect.RadiusProperty, Motion.To(ContentBlurDips, 0.25, EaseCurve));
        oldScale.BeginAnimation(ScaleTransform.ScaleXProperty, Motion.To(ContentHiddenScale, 0.35, EaseCurve));
        oldScale.BeginAnimation(ScaleTransform.ScaleYProperty, Motion.To(ContentHiddenScale, 0.35, EaseCurve));

        BlurEffect newBlur = new() { Radius = ContentBlurDips };
        newContent.Effect = newBlur;
        newContent.BeginAnimation(UIElement.OpacityProperty, Motion.To(1, 0.3, EaseCurve, ContentDelaySeconds));
        newBlur.BeginAnimation(BlurEffect.RadiusProperty, Motion.To(0, 0.35, EaseCurve, ContentDelaySeconds));
        newScale.BeginAnimation(ScaleTransform.ScaleYProperty, Motion.To(1, 0.5, SpringCurve, ContentDelaySeconds));

        DoubleAnimation lastStep = Motion.To(1, 0.5, SpringCurve, ContentDelaySeconds).Clone();
        lastStep.Completed += (_, _) =>
        {
            // The swap is over: drop the blur so the resting island is cheap to draw.
            oldContent.Effect = null;
            newContent.Effect = null;
        };
        newScale.BeginAnimation(ScaleTransform.ScaleXProperty, lastStep);
    }

    public void SetVisible(bool isVisible)
    {
        _pill.IsHitTestVisible = isVisible;
        double scale = isVisible ? 1 : HiddenScale;
        Motion.Run(_pill, UIElement.OpacityProperty, Motion.To(isVisible ? 1 : 0, HideFadeSeconds, EaseCurve));
        Motion.Run(_pillScale, ScaleTransform.ScaleXProperty, Motion.To(scale, HideMoveSeconds, SpringCurve));
        Motion.Run(_pillScale, ScaleTransform.ScaleYProperty, Motion.To(scale, HideMoveSeconds, SpringCurve));
        Motion.Run(_pillOffset, TranslateTransform.YProperty, Motion.To(isVisible ? 0 : HiddenOffsetDips, HideMoveSeconds, SpringCurve));
    }

    private static void ShowContentInstantly(FrameworkElement oldContent, FrameworkElement newContent)
    {
        foreach ((FrameworkElement content, double opacity, double scale) in new[]
                 {
                     (oldContent, 0.0, ContentHiddenScale),
                     (newContent, 1.0, 1.0),
                 })
        {
            content.Effect = null;
            content.BeginAnimation(UIElement.OpacityProperty, null);
            content.Opacity = opacity;
            ScaleTransform contentScale = ContentScale(content);
            contentScale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            contentScale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            contentScale.ScaleX = scale;
            contentScale.ScaleY = scale;
        }
    }

    private static ScaleTransform ContentScale(FrameworkElement content)
    {
        return content.RenderTransform as ScaleTransform
            ?? throw new InvalidOperationException($"{content.Name} needs a ScaleTransform as its RenderTransform.");
    }
}
