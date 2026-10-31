// MusicControlsView.xaml.cs: fills the expanded island and reports what the user clicks.
// Shows the song, the times and the progress bar (FR-7, FR-9), raises events for previous,
// play/pause and next (FR-8), and handles seeking with the mouse (FR-11): hover shows the larger
// bar and thumb, click or drag moves it, and the new position is sent once when the button is
// released, so Spotify is not flooded with requests while dragging. Without seek support the bar
// is display only.

using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using WinIsland.Core.Music;

namespace WinIsland.App.Music;

public partial class MusicControlsView : UserControl
{
    private const double BarDips = 4;
    private const double HoverBarDips = 6;
    private const double ThumbRadiusDips = 5;
    private const string PauseIconPath = "M7 5h4v14H7zM13 5h4v14h-4z";
    private const string PlayIconPath = "M7 5v14l12-7z";

    private static readonly Geometry PauseIcon = Geometry.Parse(PauseIconPath);
    private static readonly Geometry PlayIcon = Geometry.Parse(PlayIconPath);

    private NowPlaying? _music;
    private double _shownFraction;
    private double? _dragFraction;

    public MusicControlsView()
    {
        InitializeComponent();
        Artwork.Background = ArtworkBrush.For(null);
        SeekArea.SizeChanged += (_, _) => DrawProgress(_dragFraction ?? _shownFraction);
    }

    public event EventHandler? PreviousClicked;

    public event EventHandler? PlayPauseClicked;

    public event EventHandler? NextClicked;

    public event EventHandler<TimeSpan>? SeekRequested;

    public void Show(NowPlaying music, ImageSource? artwork, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(music);
        _music = music;

        TitleText.Text = SongText.TitleOf(music);
        ArtistText.Text = music.Artist;
        Artwork.Background = ArtworkBrush.For(artwork);
        TotalText.Text = TimeLabel.Format(music.Duration);

        PlayPauseIcon.Data = music.IsPlaying ? PauseIcon : PlayIcon;
        AutomationProperties.SetName(PlayPauseButton, music.IsPlaying ? "Pause" : "Play");
        SeekArea.Cursor = music.CanSeek ? Cursors.Hand : Cursors.Arrow;
        if (!music.CanSeek)
        {
            ShowHover(false);
        }

        UpdateProgress(now);
    }

    /// <summary>Moves the elapsed time and the bar on; called once per second while expanded (DESIGN_SPEC section 7).</summary>
    public void UpdateProgress(DateTimeOffset now)
    {
        if (_music is null || _dragFraction is not null)
        {
            return;
        }

        ElapsedText.Text = TimeLabel.Format(PlaybackPosition.Estimate(_music, now));
        _shownFraction = PlaybackPosition.Fraction(_music, now);
        DrawProgress(_shownFraction);
    }

    private void DrawProgress(double fraction)
    {
        double width = SeekArea.ActualWidth * Math.Clamp(fraction, 0, 1);
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

    private bool CanSeek => _music is { CanSeek: true, Duration.Ticks: > 0 };

    private double FractionAt(MouseEventArgs e)
    {
        double width = SeekArea.ActualWidth;
        return width > 0 ? e.GetPosition(SeekArea).X / width : 0;
    }

    private void OnSeekAreaMouseEnter(object sender, MouseEventArgs e)
    {
        ShowHover(CanSeek);
    }

    private void OnSeekAreaMouseLeave(object sender, MouseEventArgs e)
    {
        if (_dragFraction is null)
        {
            ShowHover(false);
        }
    }

    private void OnSeekAreaMouseDown(object sender, MouseButtonEventArgs e)
    {
        // Clicks on the bar never close the island, with or without seek support (prototype).
        e.Handled = true;
        if (!CanSeek)
        {
            return;
        }

        SeekArea.CaptureMouse();
        _dragFraction = Math.Clamp(FractionAt(e), 0, 1);
        DrawProgress(_dragFraction.Value);
    }

    private void OnSeekAreaMouseMove(object sender, MouseEventArgs e)
    {
        if (_dragFraction is null)
        {
            return;
        }

        _dragFraction = Math.Clamp(FractionAt(e), 0, 1);
        DrawProgress(_dragFraction.Value);
        if (_music is not null)
        {
            ElapsedText.Text = TimeLabel.Format(SeekPosition.ForFraction(_dragFraction.Value, _music.Duration));
        }
    }

    private void OnSeekAreaMouseUp(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        if (_dragFraction is not double fraction || _music is null)
        {
            return;
        }

        SeekArea.ReleaseMouseCapture();
        _dragFraction = null;
        _shownFraction = fraction;
        if (!SeekArea.IsMouseOver)
        {
            ShowHover(false);
        }

        SeekRequested?.Invoke(this, SeekPosition.ForFraction(fraction, _music.Duration));
    }

    private void OnPreviousClick(object sender, RoutedEventArgs e) => PreviousClicked?.Invoke(this, EventArgs.Empty);

    private void OnPlayPauseClick(object sender, RoutedEventArgs e) => PlayPauseClicked?.Invoke(this, EventArgs.Empty);

    private void OnNextClick(object sender, RoutedEventArgs e) => NextClicked?.Invoke(this, EventArgs.Empty);
}
