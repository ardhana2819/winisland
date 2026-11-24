// EqualizerView.xaml.cs: moves the equalizer bars while music plays (PRD FR-48, DESIGN_SPEC
// section 15). Each bar follows its own uneven loop of heights from Core (EqualizerPattern), eased
// in and out between them, so the bars look like music without listening to it. The loops are
// built once and shared by every equalizer. To stay light (PRD G4), they redraw at 30 frames per
// second, which looks the same for bars this small. Paused, the bars stop as dots. With Windows
// animation effects off they stand still at the heights from the design.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using WinIsland.App.Animation;
using WinIsland.Core.Music;

namespace WinIsland.App.Music;

public partial class EqualizerView : UserControl
{
    private const int FramesPerSecond = 30;

    private static readonly Lazy<AnimationTimeline[]> BarLoops = new(BuildBarLoops);

    private enum BarMode
    {
        Dots,
        Still,
        Moving,
        Held,
    }

    private readonly Rectangle[] _bars;
    private bool _isPlaying;
    private bool _isOnScreen = true;
    private BarMode _mode = BarMode.Dots;

    public EqualizerView()
    {
        InitializeComponent();
        _bars = [BarOne, BarTwo, BarThree, BarFour, BarFive];
    }

    /// <summary>The bars' color: the artwork's accent, or green without artwork (DESIGN_SPEC section 15).</summary>
    public void SetBarBrush(Brush brush)
    {
        ArgumentNullException.ThrowIfNull(brush);
        foreach (Rectangle bar in _bars)
        {
            bar.Fill = brush;
        }
    }

    public void SetPlaying(bool isPlaying)
    {
        _isPlaying = isPlaying;
        ShowMode(ModeNow());
    }

    /// <summary>
    /// Off screen, playing bars stop where they are instead of redrawing unseen (PRD G4), so the
    /// pill fading out never shows a jump; back on screen they move again.
    /// </summary>
    public void SetOnScreen(bool isOnScreen)
    {
        _isOnScreen = isOnScreen;
        ShowMode(ModeNow());
    }

    private BarMode ModeNow()
    {
        if (!_isPlaying)
        {
            return BarMode.Dots;
        }

        if (!Motion.IsEnabled)
        {
            return BarMode.Still;
        }

        // Moving bars that leave the screen hold; bars that were not moving keep their heights.
        return _isOnScreen ? BarMode.Moving : _mode == BarMode.Moving ? BarMode.Held : _mode;
    }

    private void ShowMode(BarMode mode)
    {
        if (mode == _mode)
        {
            return;
        }

        _mode = mode;
        for (int index = 0; index < _bars.Length; index++)
        {
            Rectangle bar = _bars[index];
            if (mode == BarMode.Moving)
            {
                bar.BeginAnimation(HeightProperty, BarLoops.Value[index]);
                continue;
            }

            // Read the height before the animation is removed, so a held bar keeps it.
            double height = mode switch
            {
                BarMode.Held => bar.Height,
                BarMode.Still => EqualizerPattern.StillHeightOf(index),
                _ => EqualizerPattern.LowestDips,
            };
            bar.BeginAnimation(HeightProperty, null);
            bar.Height = height;
        }
    }

    private static AnimationTimeline[] BuildBarLoops()
    {
        AnimationTimeline[] loops = new AnimationTimeline[EqualizerPattern.BarCount];
        for (int index = 0; index < loops.Length; index++)
        {
            loops[index] = BuildLoop(EqualizerPattern.LoopOf(index));
        }

        return loops;
    }

    // One bar's loop as key frames: it starts at the last step's height, so repeating has no jump.
    private static DoubleAnimationUsingKeyFrames BuildLoop(IReadOnlyList<EqualizerStep> steps)
    {
        IEasingFunction ease = CubicBezierEase.EaseInOut();
        DoubleAnimationUsingKeyFrames loop = new() { RepeatBehavior = RepeatBehavior.Forever };
        loop.KeyFrames.Add(new DiscreteDoubleKeyFrame(steps[^1].HeightDips, KeyTime.FromTimeSpan(TimeSpan.Zero)));

        double elapsedSeconds = 0;
        foreach (EqualizerStep step in steps)
        {
            elapsedSeconds += step.Seconds;
            loop.KeyFrames.Add(new EasingDoubleKeyFrame(step.HeightDips, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(elapsedSeconds)), ease));
        }

        Timeline.SetDesiredFrameRate(loop, FramesPerSecond);
        loop.Freeze();
        return loop;
    }
}
