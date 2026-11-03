// MusicSourceTests.cs: checks which app's music the island shows (PRD FR-41, v1.5). Before v1.5
// these tests checked "Spotify only"; FR-41 replaced that rule with music from every app.

using WinIsland.Core.Music;

namespace WinIsland.Core.Tests.Music;

public class MusicSourceTests
{
    [Fact]
    public void The_app_that_plays_is_shown_even_when_Windows_marks_another()
    {
        MediaSessionCandidate[] sessions = [new("Spotify.exe", IsPlaying: false), new("Chrome", IsPlaying: true)];

        Assert.Equal("Chrome", MusicSource.Choose(sessions, windowsCurrentAppId: "Spotify.exe"));
    }

    [Fact]
    public void With_several_playing_the_one_Windows_marks_wins()
    {
        MediaSessionCandidate[] sessions = [new("Spotify.exe", IsPlaying: true), new("Chrome", IsPlaying: true)];

        Assert.Equal("Chrome", MusicSource.Choose(sessions, windowsCurrentAppId: "Chrome"));
        Assert.Equal("Spotify.exe", MusicSource.Choose(sessions, windowsCurrentAppId: "Brave"));
    }

    [Fact]
    public void With_several_playing_the_app_on_the_island_stays()
    {
        MediaSessionCandidate[] sessions = [new("Spotify.exe", IsPlaying: true), new("Brave", IsPlaying: true)];

        Assert.Equal("Spotify.exe", MusicSource.Choose(sessions, windowsCurrentAppId: "Brave", shownAppId: "Spotify.exe"));
    }

    [Fact]
    public void The_app_on_the_island_gives_way_once_it_stops()
    {
        MediaSessionCandidate[] sessions = [new("Spotify.exe", IsPlaying: false), new("Brave", IsPlaying: true)];

        Assert.Equal("Brave", MusicSource.Choose(sessions, windowsCurrentAppId: "Spotify.exe", shownAppId: "Spotify.exe"));
    }

    [Fact]
    public void With_none_playing_the_one_Windows_marks_is_shown_paused()
    {
        MediaSessionCandidate[] sessions = [new("Spotify.exe", IsPlaying: false), new("AppleInc.AppleMusicWin_nzyj5cx40ttqa!App", IsPlaying: false)];

        Assert.Equal("AppleInc.AppleMusicWin_nzyj5cx40ttqa!App", MusicSource.Choose(sessions, "AppleInc.AppleMusicWin_nzyj5cx40ttqa!App"));
        Assert.Equal("Spotify.exe", MusicSource.Choose(sessions, windowsCurrentAppId: null));
    }

    [Fact]
    public void No_sessions_means_no_music()
    {
        Assert.Null(MusicSource.Choose([], "Spotify.exe"));
    }

    [Theory]
    [InlineData("Spotify.exe", "Spotify", null, true)]
    [InlineData("Chrome", "chrome", null, true)]
    [InlineData("Chrome._crx_cinhimbnkkaeohfgghhklpknlkffjgod", "chrome", null, true)] // YouTube Music installed from Chrome
    [InlineData("MSEdge", "msedge", null, true)]
    [InlineData("AppleInc.AppleMusicWin_nzyj5cx40ttqa!App", "AppleMusic", "AppleInc.AppleMusicWin_nzyj5cx40ttqa", true)]
    [InlineData("Spotify.exe", "chrome", null, false)]
    [InlineData("AppleInc.AppleMusicWin_nzyj5cx40ttqa!App", "AppleMusic", null, false)]
    [InlineData("308046B0AF4A39CB", "firefox", null, false)] // Firefox names itself with a code
    [InlineData("", "Spotify", null, false)]
    public void The_music_apps_sound_is_found_in_the_mixer(string mediaAppId, string processName, string? packageFamily, bool isMatch)
    {
        Assert.Equal(isMatch, AudioAppMatch.IsMusicApp(mediaAppId, processName, packageFamily));
    }
}
