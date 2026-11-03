// MusicPillView.xaml.cs: fills the compact music pill with the current song.

using System.Windows.Controls;
using System.Windows.Media;

using WinIsland.Core.Music;

namespace WinIsland.App.Music;

public partial class MusicPillView : UserControl
{
    public MusicPillView()
    {
        InitializeComponent();
        Artwork.Background = ArtworkBrush.For(null);
    }

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

    /// <summary>The title only scrolls while this pill is the view on screen.</summary>
    public void SetActive(bool isActive) => TitleText.SetActive(isActive);
}
