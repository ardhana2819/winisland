// MusicControlsView.xaml.cs: fills the expanded island and reports what the user clicks.
// Shows the song, the times and the progress bar (FR-7, FR-9), raises events for previous,
// play/pause and next (FR-8), and handles seeking with the mouse (FR-11): hover shows the larger
// bar and thumb, click or drag moves it, and the new position is sent once when the button is
// released, so the music app is not flooded with requests while dragging. Without seek support the bar
// is display only. Without a music session it shows the empty view instead (FR-24).
// Colors come from the artwork (FR-25, DESIGN_SPEC section 9): the icons, play circle, progress
// fill, thumb and equalizer use the accent, the artist and times the grey text color, and every
// change fades over 0.45 s.
// v1.5 (FR-39): the speaker button asks for the volume row; the row swaps in for the progress row
// (RowSwap) and reports levels the user picks. The presenter decides when it opens and closes.

using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using WinIsland.App.Animation;
using WinIsland.Core.Music;

namespace WinIsland.App.Music;

public partial class MusicControlsView : UserControl
{
    private const double BarDips = 4;
    private const double HoverBarDips = 6;
    private const double ThumbRadiusDips = 5;
    private const double EmptyControlsOpacity = 0.35;
    private const string EmptyTitle = "Not playing";
    private const string PauseIconPath = "M7 5h4v14H7zM13 5h4v14h-4z";
    private const string PlayIconPath = "M7 5v14l12-7z";

    private static readonly Geometry SpeakerMuted = Geometry.Parse("M3 9v6h4l5 5V4L7 9zm13.6 3 2.7-2.7-1.2-1.2-2.7 2.7-2.7-2.7-1.2 1.2 2.7 2.7-2.7 2.7 1.2 1.2 2.7-2.7 2.7 2.7 1.2-1.2z");
    private static readonly Geometry SpeakerLow = Geometry.Parse("M3 9v6h4l5 5V4L7 9zm13.5 3A4.5 4.5 0 0 0 14 8v8a4.5 4.5 0 0 0 2.5-4z");
    private static readonly Geometry SpeakerHigh = Geometry.Parse("M3 9v6h4l5 5V4L7 9zm13.5 3A4.5 4.5 0 0 0 14 8v8a4.5 4.5 0 0 0 2.5-4zM14 3.2v2.1a7 7 0 0 1 0 13.4v2.1a9 9 0 0 0 0-17.6z");
    private static readonly Geometry PauseIcon = Geometry.Parse(PauseIconPath);
    private static readonly Geometry PlayIcon = Geometry.Parse(PlayIconPath);

    // Owned, not frozen, so their colors can fade.
    private readonly SolidColorBrush _accent = new(Colors.White);
    private readonly SolidColorBrush _accentSoft = new(Colors.White);
    private readonly SolidColorBrush _secondary = new(Colors.White);

    private NowPlaying? _music;
    private double _shownFraction;
    private double? _dragFraction;
    private bool _isActive;
    private ArtworkPalette _shownPalette = ArtworkPalette.Black;
    private bool _isVolumeShown;

    public MusicControlsView()
    {
        InitializeComponent();
        Artwork.Background = ArtworkBrush.For(null);
        SeekArea.SizeChanged += (_, _) => DrawProgress(_dragFraction ?? _shownFraction);

        PreviousIcon.Fill = _accent;
        PlayPauseIcon.Fill = _accent;
        NextIcon.Fill = _accent;
        SpeakerIcon.Fill = _accent;
        RowSwap.Set(ProgressRow, true);
        RowSwap.Set(VolumeRow, false);
        VolumeRow.LevelChosen += (_, level) => VolumeChosen?.Invoke(this, level);
        ElapsedText.SizeChanged += (_, _) => VolumeRow.MatchSlots(ElapsedText.ActualWidth, TotalText.ActualWidth);
        TotalText.SizeChanged += (_, _) => VolumeRow.MatchSlots(ElapsedText.ActualWidth, TotalText.ActualWidth);
        Fill.Background = _accent;
        Thumb.Fill = _accent;
        PlayPauseButton.Background = _accentSoft;
        ArtistText.Foreground = _secondary;
        ElapsedText.Foreground = _secondary;
        TotalText.Foreground = _secondary;
        Equalizer.SetBarBrush(_accent);
        ShowPalette(ArtworkPalette.Black, fade: false);
    }

    /// <summary>The colors from the song's artwork, or ArtworkPalette.Black without one (FR-25).</summary>
    public void ApplyPalette(ArtworkPalette palette)
    {
        ArgumentNullException.ThrowIfNull(palette);
        if (palette == _shownPalette)
        {
            return;
        }

        _shownPalette = palette;
        ShowPalette(palette, fade: true);
    }

    private void ShowPalette(ArtworkPalette palette, bool fade)
    {
        Color accent = ToColor(palette.Accent);
        Color accentSoft = Color.FromArgb((byte)Math.Round(ArtworkPalette.AccentSoftOpacity * 255), accent.R, accent.G, accent.B);
        Color secondary = ToColor(palette.Secondary);
        if (fade)
        {
            ColorFade.To(_accent, accent);
            ColorFade.To(_accentSoft, accentSoft);
            ColorFade.To(_secondary, secondary);
            return;
        }

        _accent.Color = accent;
        _accentSoft.Color = accentSoft;
        _secondary.Color = secondary;
    }

    private static Color ToColor(RgbColor color) => Color.FromRgb(color.R, color.G, color.B);

    public event EventHandler? PreviousClicked;

    public event EventHandler? PlayPauseClicked;

    public event EventHandler? NextClicked;

    public event EventHandler<TimeSpan>? SeekRequested;

    /// <summary>The speaker button was clicked (FR-39).</summary>
    public event EventHandler? SpeakerClicked;

    /// <summary>A volume level, 0 to 1, picked on the volume row (FR-39).</summary>
    public event EventHandler<float>? VolumeChosen;

    /// <summary>Swaps between the progress row and the volume row; level null means no sound open with Windows.</summary>
    public void ShowVolume(bool isShown, float? level)
    {
        if (isShown)
        {
            VolumeRow.Show(level);
        }

        if (level is float known)
        {
            SpeakerIcon.Data = known <= 0 ? SpeakerMuted : known < 0.5f ? SpeakerLow : SpeakerHigh;
        }

        SpeakerButton.Background = isShown ? _accentSoft : Brushes.Transparent;
        if (isShown == _isVolumeShown)
        {
            return;
        }

        _isVolumeShown = isShown;
        if (isShown)
        {
            RowSwap.Swap(ProgressRow, VolumeRow);
        }
        else
        {
            RowSwap.Swap(VolumeRow, ProgressRow);
        }
    }

    public void Show(NowPlaying music, ImageSource? artwork, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(music);
        _music = music;
        SetEmptyLook(false);

        TitleText.Text = SongText.TitleOf(music);
        ArtistText.Text = music.Artist;
        Artwork.Background = ArtworkBrush.For(artwork);
        TotalText.Text = TimeLabel.Format(music.Duration);

        PlayPauseIcon.Data = music.IsPlaying ? PauseIcon : PlayIcon;
        AutomationProperties.SetName(PlayPauseButton, music.IsPlaying ? "Pause" : "Play");
        Equalizer.SetPlaying(_isActive && music.IsPlaying);
        SeekArea.Cursor = music.CanSeek ? Cursors.Hand : Cursors.Arrow;
        if (!music.CanSeek)
        {
            ShowHover(false);
        }

        UpdateProgress(now);
    }

    /// <summary>The empty view when there is no music session, for example the music app closed (FR-24).</summary>
    public void ShowEmpty()
    {
        _music = null;
        _dragFraction = null;
        // The music app can close in the middle of a drag; let go of the mouse so the island stays clickable.
        if (SeekArea.IsMouseCaptured)
        {
            SeekArea.ReleaseMouseCapture();
        }

        SetEmptyLook(true);
        TitleText.Text = EmptyTitle;
        ArtistText.Text = string.Empty;
        Artwork.Background = ArtworkBrush.For(null);
        PlayPauseIcon.Data = PlayIcon;
        AutomationProperties.SetName(PlayPauseButton, "Play");
    }

    private void SetEmptyLook(bool isEmpty)
    {
        ProgressRow.Visibility = isEmpty ? Visibility.Collapsed : Visibility.Visible;
        // No speaker and no volume in the empty view (FR-39).
        SpeakerButton.Visibility = isEmpty ? Visibility.Collapsed : Visibility.Visible;
        if (isEmpty)
        {
            ShowVolume(false, null);
        }

        // An empty artist line still takes a line of height and pushes "Not playing" above the
        // artwork's middle, so the empty view leaves it out.
        ArtistText.Visibility = isEmpty ? Visibility.Collapsed : Visibility.Visible;
        Equalizer.Visibility = isEmpty ? Visibility.Collapsed : Visibility.Visible;
        if (isEmpty)
        {
            Equalizer.SetPlaying(false);
        }
        ControlsRow.Opacity = isEmpty ? EmptyControlsOpacity : 1;
        ControlsRow.IsEnabled = !isEmpty;
    }

    /// <summary>
    /// The title only scrolls, and the equalizer only moves, while the expanded island is the view
    /// on screen, so nothing animates unseen (PRD G4).
    /// </summary>
    public void SetActive(bool isActive)
    {
        _isActive = isActive;
        TitleText.SetActive(isActive);
        Equalizer.SetPlaying(isActive && _music is { IsPlaying: true });
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

    private void OnSpeakerClick(object sender, RoutedEventArgs e) => SpeakerClicked?.Invoke(this, EventArgs.Empty);
}
