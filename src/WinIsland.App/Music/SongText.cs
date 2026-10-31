// SongText.cs: the title text shown for a song.
// Spotify always sends a title for songs, but ads and some podcast moments can come without one.
// Then the island shows "Spotify" rather than an empty pill.

using WinIsland.Core.Music;

namespace WinIsland.App.Music;

internal static class SongText
{
    private const string MissingTitle = "Spotify";

    public static string TitleOf(NowPlaying music)
    {
        return string.IsNullOrWhiteSpace(music.Title) ? MissingTitle : music.Title;
    }
}
