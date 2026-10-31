// MusicSourceTests.cs: checks that only Spotify's media session is picked (PRD: Spotify desktop only).

using WinIsland.Core.Music;

namespace WinIsland.Core.Tests.Music;

public class MusicSourceTests
{
    [Theory]
    [InlineData("Spotify.exe")] // installer version, seen in spike U6
    [InlineData("SpotifyAB.SpotifyMusic_zpdnekdrzrea0!Spotify")] // Microsoft Store version
    [InlineData("spotify.exe")]
    public void Spotify_sessions_are_picked(string appId)
    {
        Assert.True(MusicSource.IsSpotify(appId));
    }

    [Theory]
    [InlineData("Brave")] // Windows chose this as "current session" in the spike
    [InlineData("Chrome")]
    [InlineData("Microsoft.ZuneMusic_8wekyb3d8bbwe!Microsoft.ZuneMusic")]
    [InlineData("")]
    [InlineData(null)]
    public void Other_sessions_are_ignored(string? appId)
    {
        Assert.False(MusicSource.IsSpotify(appId));
    }
}
