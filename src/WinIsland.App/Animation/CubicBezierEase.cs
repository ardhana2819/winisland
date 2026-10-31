// CubicBezierEase.cs: lets WPF animations use the design's timing curves.
// WPF's own easing functions cannot overshoot the way the spring curve does, so this wraps the
// curve math from Core (WinIsland.Core.Animation.CubicBezier).
// The curve numbers are dependency properties because WPF copies easing functions internally,
// and plain fields would be lost in the copy.

using System.Windows;
using System.Windows.Media.Animation;

using WinIsland.Core.Animation;

namespace WinIsland.App.Animation;

internal sealed class CubicBezierEase : EasingFunctionBase
{
    public static readonly DependencyProperty X1Property = DependencyProperty.Register(nameof(X1), typeof(double), typeof(CubicBezierEase));
    public static readonly DependencyProperty Y1Property = DependencyProperty.Register(nameof(Y1), typeof(double), typeof(CubicBezierEase));
    public static readonly DependencyProperty X2Property = DependencyProperty.Register(nameof(X2), typeof(double), typeof(CubicBezierEase));
    public static readonly DependencyProperty Y2Property = DependencyProperty.Register(nameof(Y2), typeof(double), typeof(CubicBezierEase));

    private CubicBezier? _curve;

    public CubicBezierEase()
    {
        // EaseIn means "use the curve exactly as given"; WPF would otherwise mirror it.
        EasingMode = EasingMode.EaseIn;
    }

    /// <summary>DESIGN_SPEC section 6 spring curve: cubic-bezier(0.34, 1.4, 0.56, 1).</summary>
    public static CubicBezierEase Spring() => Create(0.34, 1.4, 0.56, 1);

    /// <summary>CSS "ease", which the prototype uses for its fades.</summary>
    public static CubicBezierEase Ease() => Create(0.25, 0.1, 0.25, 1);

    /// <summary>CSS "ease-in-out", which the prototype uses for the equalizer bars.</summary>
    public static CubicBezierEase EaseInOut() => Create(0.42, 0, 0.58, 1);

    public double X1 { get => (double)GetValue(X1Property); set => SetValue(X1Property, value); }

    public double Y1 { get => (double)GetValue(Y1Property); set => SetValue(Y1Property, value); }

    public double X2 { get => (double)GetValue(X2Property); set => SetValue(X2Property, value); }

    public double Y2 { get => (double)GetValue(Y2Property); set => SetValue(Y2Property, value); }

    protected override double EaseInCore(double normalizedTime)
    {
        _curve ??= new CubicBezier(X1, Y1, X2, Y2);
        return _curve.Evaluate(normalizedTime);
    }

    protected override Freezable CreateInstanceCore() => new CubicBezierEase();

    private static CubicBezierEase Create(double x1, double y1, double x2, double y2)
    {
        CubicBezierEase ease = new() { X1 = x1, Y1 = y1, X2 = x2, Y2 = y2 };
        ease.Freeze();
        return ease;
    }
}
