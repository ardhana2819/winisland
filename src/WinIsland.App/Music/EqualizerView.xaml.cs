// EqualizerView.xaml.cs: moves the equalizer bars while music plays (DESIGN_SPEC section 6).
// Each bar grows between 4 and 16 high and back over 0.9 s, the three bars out of step by 0.3 s.
// The bars stop when music is paused. To stay light (PRD G4), they redraw at 30 frames per
// second, which looks the same for bars this small. With Windows animation effects off they stand
// still at the heights from the design board.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using WinIsland.App.Animation;

namespace WinIsland.App.Music;

public partial class EqualizerView : UserControl
{
    private const double LowDips = 4;
    private const double HighDips = 16;
    private const double PausedDips = 5;
    private const double CycleSeconds = 0.9;
    private const int FramesPerSecond = 30;

    // Design board heights for a still equalizer.
    private static readonly double[] StillHeights = [10, 16, 8];
    private static readonly double[] StartOffsetsSeconds = [0, 0.3, 0.6];

    private bool _isPlaying;

    public EqualizerView()
    {
        InitializeComponent();
    }

    /// <summary>The bars' color: green in the compact pill, the accent on the music page (DESIGN_SPEC section 9).</summary>
    public void SetBarBrush(Brush brush)
    {
        ArgumentNullException.ThrowIfNull(brush);
        BarOne.Fill = brush;
        BarTwo.Fill = brush;
        BarThree.Fill = brush;
    }

    public void SetPlaying(bool isPlaying)
    {
        if (isPlaying == _isPlaying)
        {
            return;
        }

        _isPlaying = isPlaying;
        Rectangle[] bars = [BarOne, BarTwo, BarThree];
        for (int index = 0; index < bars.Length; index++)
        {
            Rectangle bar = bars[index];
            bar.ApplyAnimationClock(HeightProperty, null);

            if (!isPlaying)
            {
                bar.Height = PausedDips;
            }
            else if (!Motion.IsEnabled)
            {
                bar.Height = StillHeights[index];
            }
            else
            {
                StartBar(bar, StartOffsetsSeconds[index]);
            }
        }
    }

    private static void StartBar(Rectangle bar, double offsetSeconds)
    {
        DoubleAnimation bounce = new(LowDips, HighDips, TimeSpan.FromSeconds(CycleSeconds))
        {
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = CubicBezierEase.EaseInOut(),
        };
        Timeline.SetDesiredFrameRate(bounce, FramesPerSecond);

        AnimationClock clock = bounce.CreateClock();
        bar.ApplyAnimationClock(HeightProperty, clock);
        // Start each bar part-way through its cycle, like the prototype's negative delays.
        clock.Controller?.Seek(TimeSpan.FromSeconds(offsetSeconds), TimeSeekOrigin.BeginTime);
    }
}
