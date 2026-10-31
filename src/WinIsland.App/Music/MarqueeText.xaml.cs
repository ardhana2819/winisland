// MarqueeText.xaml.cs: scrolls a line of text when it does not fit, and only then.
// Each loop: hold still for a moment so the start can be read, then glide left at a steady speed
// until the second copy sits exactly where the first one started, and repeat. Short text never
// moves. To stay light (PRD G4) it only moves while its view is on screen (SetActive), redraws at
// 30 frames per second, and falls back to an ellipsis when Windows animation effects are off.

using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

using WinIsland.App.Animation;

namespace WinIsland.App.Music;

public partial class MarqueeText : UserControl
{
    private const double GapDips = 40;
    private const double SpeedDipsPerSecond = 30;
    private const double PauseSeconds = 1.5;
    private const int FramesPerSecond = 30;

    private bool _isActive;

    public MarqueeText()
    {
        InitializeComponent();
        SecondCopy.Margin = new Thickness(GapDips, 0, 0, 0);
        Viewport.SizeChanged += (_, _) => Restart();
    }

    public string Text
    {
        get => FirstCopy.Text;
        set
        {
            string text = value ?? string.Empty;
            if (text == FirstCopy.Text)
            {
                return;
            }

            FirstCopy.Text = text;
            SecondCopy.Text = text;
            AutomationProperties.SetName(this, text);
            Restart();
        }
    }

    /// <summary>Scrolls only while the view holding this text is the one on screen.</summary>
    public void SetActive(bool isActive)
    {
        if (isActive == _isActive)
        {
            return;
        }

        _isActive = isActive;
        Restart();
    }

    private void Restart()
    {
        Offset.BeginAnimation(TranslateTransform.XProperty, null);
        Offset.X = 0;

        double available = Viewport.ActualWidth;
        Size textSize = MeasureText();
        double textWidth = textSize.Width;
        bool overflows = available > 0 && textWidth > available;

        // The Canvas reports no size of its own, so the visible area takes the text's height.
        Viewport.Height = textSize.Height;

        if (!overflows || !_isActive || !Motion.IsEnabled)
        {
            // Still text: cut cleanly with an ellipsis if it is too long (the design's default).
            SecondCopy.Visibility = Visibility.Collapsed;
            FirstCopy.MaxWidth = available > 0 ? available : double.PositiveInfinity;
            return;
        }

        FirstCopy.MaxWidth = double.PositiveInfinity;
        SecondCopy.Visibility = Visibility.Visible;

        double distance = textWidth + GapDips;
        double glideSeconds = distance / SpeedDipsPerSecond;
        DoubleAnimationUsingKeyFrames loop = new()
        {
            Duration = TimeSpan.FromSeconds(PauseSeconds + glideSeconds),
            RepeatBehavior = RepeatBehavior.Forever,
        };
        loop.KeyFrames.Add(new DiscreteDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        loop.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(PauseSeconds))));
        loop.KeyFrames.Add(new LinearDoubleKeyFrame(-distance, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(PauseSeconds + glideSeconds))));
        Timeline.SetDesiredFrameRate(loop, FramesPerSecond);
        Offset.BeginAnimation(TranslateTransform.XProperty, loop);
    }

    private Size MeasureText()
    {
        FirstCopy.MaxWidth = double.PositiveInfinity;
        FirstCopy.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        return FirstCopy.DesiredSize;
    }
}
