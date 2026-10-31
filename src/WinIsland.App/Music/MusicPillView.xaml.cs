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
}
