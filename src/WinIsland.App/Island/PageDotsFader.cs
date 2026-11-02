// PageDotsFader.cs: shows the page dots only while they help (PRD FR-31, DESIGN_SPEC section 10).
// They fade in over 0.2 s when the island opens and on every page change, and fade out 1.5 s
// after the last change. Pointing at the island does not bring them back (Diva, 2 Nov 2026).
// Closing the island hides them at once, so the next opening fades them in again. With Windows
// animation effects off they appear and disappear at once (Motion).

using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

using WinIsland.App.Animation;

namespace WinIsland.App.Island;

internal sealed class PageDotsFader
{
    private static readonly TimeSpan ShownTime = TimeSpan.FromSeconds(1.5);
    private const double FadeSeconds = 0.2;

    private static readonly IEasingFunction EaseCurve = CubicBezierEase.Ease();

    private readonly UIElement _dots;
    private readonly DispatcherTimer _hideTimer;

    public PageDotsFader(UIElement dots)
    {
        ArgumentNullException.ThrowIfNull(dots);
        _dots = dots;
        _dots.Opacity = 0;

        _hideTimer = new DispatcherTimer(DispatcherPriority.Normal, dots.Dispatcher) { Interval = ShownTime };
        _hideTimer.Tick += (_, _) =>
        {
            _hideTimer.Stop();
            FadeTo(0);
        };
    }

    /// <summary>The island opened or changed page: show the dots, then fade them out 1.5 s later.</summary>
    public void Flash()
    {
        FadeTo(1);
        _hideTimer.Stop();
        _hideTimer.Start();
    }

    /// <summary>The island closed: hide the dots at once.</summary>
    public void HideNow()
    {
        _hideTimer.Stop();
        _dots.BeginAnimation(UIElement.OpacityProperty, null);
        _dots.Opacity = 0;
    }

    private void FadeTo(double opacity)
    {
        Motion.Run(_dots, UIElement.OpacityProperty, Motion.To(opacity, FadeSeconds, EaseCurve));
    }
}
