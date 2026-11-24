// MusicPillView.xaml.cs: fills the compact music pill with the current song.
// v1.9 (FR-48): the equalizer bars take the artwork's accent, fading when the song changes.

using System.Windows.Controls;
using System.Windows.Media;

using WinIsland.App.Animation;

using WinIsland.Core.Music;

namespace WinIsland.App.Music;

public partial class MusicPillView : UserControl
{
    private readonly SolidColorBrush _equalizerBrush = new(ToColor(EqualizerColor.NoArtwork));
    private RgbColor _equalizerColor = EqualizerColor.NoArtwork;

    public MusicPillView()
    {
        InitializeComponent();
        Artwork.Background = ArtworkBrush.For(null);
        Equalizer.SetBarBrush(_equalizerBrush);
    }

    /// <summary>The equalizer bars' color (FR-48); a new color fades in over 0.45 s.</summary>
    public void SetEqualizerColor(RgbColor color)
    {
        // Refresh runs on every island change; only a new color starts a fade.
        if (color == _equalizerColor)
        {
            return;
        }

        _equalizerColor = color;
        ColorFade.To(_equalizerBrush, ToColor(color));
    }

    private static Color ToColor(RgbColor color) => Color.FromRgb(color.R, color.G, color.B);

    public void Show(NowPlaying music, ImageSource? artwork)
    {
        ArgumentNullException.ThrowIfNull(music);
        TitleText.Text = SongText.TitleOf(music);
        Artwork.Background = ArtworkBrush.For(artwork);
        Equalizer.SetPlaying(music.IsPlaying);
    }

    /// <summary>Shows or hides the unread dot at the end of the pill (FR-33).</summary>
    public void SetUnread(bool hasUnread) => UnreadDot.SetShown(hasUnread);

    /// <summary>The camera or microphone dot (FR-40); null hides it.</summary>
    public void SetPrivacy(Brush? color) => PrivacyDot.Show(color);

    /// <summary>The title only scrolls, and the bars only move, while this pill is the view on screen.</summary>
    public void SetActive(bool isActive)
    {
        TitleText.SetActive(isActive);
        Equalizer.SetOnScreen(isActive);
    }
}
