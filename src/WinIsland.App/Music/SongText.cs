// SongText.cs: the title text shown for a song.
// Spotify always sends a title for songs, but ads and some podcast moments can come without one.
// Then the island shows "Now playing" rather than an empty pill. (Before v1.5 it said "Spotify";
// with music from every app, FR-41, the app is not always Spotify.)

using WinIsland.Core.Music;

namespace WinIsland.App.Music;

internal static class SongText
{
    private const string MissingTitle = "Now playing";

    public static string TitleOf(NowPlaying music)
    {
        return string.IsNullOrWhiteSpace(music.Title) ? MissingTitle : music.Title;
    }
}
