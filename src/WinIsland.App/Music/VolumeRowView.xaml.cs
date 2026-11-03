// VolumeRowView.xaml.cs: shows the music app's volume and turns clicks and drags on the bar into
// new levels (PRD FR-39). While dragging, each new level is reported at once, so the music gets
// louder or quieter under the finger, like the Volume Mixer. Without a sound open with Windows the
// row is dimmed and does nothing.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WinIsland.App.Music;

public partial class VolumeRowView : UserControl
{
    private const double BarDips = 4;
    private const double HoverBarDips = 6;
    private const double ThumbRadiusDips = 5;
    private const double UnavailableOpacity = 0.35;
    private const double SmallestStep = 0.01;

    private double _level;
    private bool _isDragging;

    public VolumeRowView()
    {
        InitializeComponent();
        BarArea.SizeChanged += (_, _) => Draw(_level);
    }

    /// <summary>A new level, 0 to 1, from a click or drag.</summary>
    public event EventHandler<float>? LevelChosen;

    /// <summary>Shows a level, 0 to 1; null when the app has no sound open with Windows.</summary>
    public void Show(float? level)
    {
        bool isAvailable = level is not null;
        Opacity = isAvailable ? 1 : UnavailableOpacity;
        BarArea.IsHitTestVisible = isAvailable;
        if (!_isDragging)
        {
            _level = level ?? 0;
            Draw(_level);
        }
    }

    /// <summary>Keeps the speaker symbols in the time labels' slots, so the bar matches the progress bar.</summary>
    public void MatchSlots(double leftWidth, double rightWidth)
    {
        LeftSlot.Width = new GridLength(leftWidth);
        RightSlot.Width = new GridLength(rightWidth);
    }

    private void Draw(double level)
    {
        double width = BarArea.ActualWidth * Math.Clamp(level, 0, 1);
        Fill.Width = width;
        Canvas.SetLeft(Thumb, width - ThumbRadiusDips);
    }

    private void ShowHover(bool isHovering)
    {
        double height = isHovering ? HoverBarDips : BarDips;
        Track.Height = height;
        Fill.Height = height;
        Thumb.Visibility = isHovering ? Visibility.Visible : Visibility.Hidden;
    }

    private void Choose(MouseEventArgs e)
    {
        double width = BarArea.ActualWidth;
        double level = width > 0 ? Math.Clamp(e.GetPosition(BarArea).X / width, 0, 1) : 0;
        if (Math.Abs(level - _level) < SmallestStep && level is > 0 and < 1)
        {
            return;
        }

        _level = level;
        Draw(level);
        LevelChosen?.Invoke(this, (float)level);
    }

    private void OnBarMouseEnter(object sender, MouseEventArgs e) => ShowHover(true);

    private void OnBarMouseLeave(object sender, MouseEventArgs e)
    {
        if (!_isDragging)
        {
            ShowHover(false);
        }
    }

    private void OnBarMouseDown(object sender, MouseButtonEventArgs e)
    {
        // Clicks on the bar never close the island.
        e.Handled = true;
        _isDragging = true;
        BarArea.CaptureMouse();
        Choose(e);
    }

    private void OnBarMouseMove(object sender, MouseEventArgs e)
    {
        if (_isDragging)
        {
            Choose(e);
        }
    }

    private void OnBarMouseUp(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        if (!_isDragging)
        {
            return;
        }

        _isDragging = false;
        BarArea.ReleaseMouseCapture();
        if (!BarArea.IsMouseOver)
        {
            ShowHover(false);
        }
    }
}
