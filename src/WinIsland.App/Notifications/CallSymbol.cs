// CallSymbol.cs: the green call circle of the call banner and the call card (PRD FR-38,
// DESIGN_SPEC section 11). Green #30D158 with a white phone for a voice call, or a white video
// camera for a video call, so the two are never mixed up (Diva, 2 Nov 2026).
// On the banner it pulses gently while the call rings: a green ring grows by 7 and fades out,
// every 1.6 seconds. The pulse runs only while Start was called and stops with Stop, so a hidden
// banner costs nothing; with Windows animation effects off it does not pulse at all.
// The two symbols are Material Icons paths (Apache License 2.0).

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using WinIsland.App.Animation;

namespace WinIsland.App.Notifications;

internal sealed class CallSymbol : Grid
{
    private const string PhonePath = "M6.6 10.8c1.4 2.8 3.8 5.1 6.6 6.6l2.2-2.2c.3-.3.7-.4 1-.2 1.1.4 2.3.6 3.6.6.6 0 1 .4 1 1V20c0 .6-.4 1-1 1-9.4 0-17-7.6-17-17 0-.6.4-1 1-1h3.5c.6 0 1 .4 1 1 0 1.3.2 2.5.6 3.6.1.3 0 .7-.2 1z";
    private const string CameraPath = "M17 10.5V7c0-.55-.45-1-1-1H4c-.55 0-1 .45-1 1v10c0 .55.45 1 1 1h12c.55 0 1-.45 1-1v-3.5l4 4v-11z";
    private const double PulseSeconds = 1.6;
    private const double PulseGrowDips = 7;

    private static readonly Brush Green = Frozen(Color.FromRgb(0x30, 0xD1, 0x58));
    private static readonly Geometry Phone = FrozenGeometry(PhonePath);
    private static readonly Geometry Camera = FrozenGeometry(CameraPath);

    private readonly Ellipse _ring;
    private readonly ScaleTransform _ringScale = new(1, 1);
    private readonly Path _glyph;
    private bool _isPulsing;

    /// <summary>size is the circle (36 on the banner, 30 on a card); glyphSize the symbol (20 or 16).</summary>
    public CallSymbol(double size, double glyphSize)
    {
        Width = size;
        Height = size;
        IsHitTestVisible = false;

        _ring = new Ellipse { Fill = Green, Opacity = 0, RenderTransform = _ringScale, RenderTransformOrigin = new Point(0.5, 0.5) };
        Children.Add(_ring);
        Children.Add(new Ellipse { Fill = Green });

        _glyph = new Path { Fill = Brushes.White, Stretch = Stretch.Uniform, Width = glyphSize, Height = glyphSize, Data = Phone };
        Children.Add(_glyph);
    }

    public void ShowKind(bool isVideo)
    {
        _glyph.Data = isVideo ? Camera : Phone;
    }

    public void Start()
    {
        if (_isPulsing || !Motion.IsEnabled)
        {
            return;
        }

        _isPulsing = true;
        double grow = (Width + (2 * PulseGrowDips)) / Width;
        Pulse(_ringScale, ScaleTransform.ScaleXProperty, 1, grow);
        Pulse(_ringScale, ScaleTransform.ScaleYProperty, 1, grow);
        Pulse(_ring, OpacityProperty, 0.55, 0);
    }

    public void Stop()
    {
        if (!_isPulsing)
        {
            return;
        }

        _isPulsing = false;
        _ringScale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
        _ringScale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
        _ring.BeginAnimation(OpacityProperty, null);
        _ring.Opacity = 0;
    }

    private static void Pulse(IAnimatable target, DependencyProperty property, double from, double to)
    {
        DoubleAnimation pulse = new(from, to, TimeSpan.FromSeconds(PulseSeconds))
        {
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = CubicBezierEase.EaseInOut(),
        };
        pulse.Freeze();
        target.BeginAnimation(property, pulse);
    }

    private static Brush Frozen(Color color)
    {
        SolidColorBrush brush = new(color);
        brush.Freeze();
        return brush;
    }

    private static Geometry FrozenGeometry(string path)
    {
        Geometry geometry = Geometry.Parse(path);
        geometry.Freeze();
        return geometry;
    }
}
