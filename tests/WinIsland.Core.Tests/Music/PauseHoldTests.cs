// PauseHoldTests.cs: checks that a paused app keeps the island for 3 seconds before another app may
// take its place, and that playing again within them cancels the hold (PRD FR-41).

using WinIsland.Core.Music;

namespace WinIsland.Core.Tests.Music;

public class PauseHoldTests
{
    private static readonly DateTimeOffset Start = new(2026, 11, 3, 20, 0, 0, TimeSpan.Zero);

    [Fact]
    public void A_pause_holds_the_app_for_3_seconds()
    {
        PauseHold hold = new();
        hold.Update("Spotify.exe", isShownPlaying: true, Start);

        Assert.Equal(TimeSpan.FromSeconds(3), hold.Update("Spotify.exe", isShownPlaying: false, Start));
        Assert.Equal(TimeSpan.FromSeconds(1), hold.Update("Spotify.exe", isShownPlaying: false, Start.AddSeconds(2)));
        Assert.Equal(TimeSpan.Zero, hold.Update("Spotify.exe", isShownPlaying: false, Start.AddSeconds(3)));
    }

    [Fact]
    public void Playing_again_within_3_seconds_ends_the_hold()
    {
        PauseHold hold = new();
        hold.Update("Spotify.exe", isShownPlaying: true, Start);
        hold.Update("Spotify.exe", isShownPlaying: false, Start);

        Assert.Equal(TimeSpan.Zero, hold.Update("Spotify.exe", isShownPlaying: true, Start.AddSeconds(1)));
        Assert.Equal(TimeSpan.FromSeconds(3), hold.Update("Spotify.exe", isShownPlaying: false, Start.AddSeconds(10)));
    }

    [Fact]
    public void An_app_that_was_never_seen_playing_is_not_held()
    {
        PauseHold hold = new();

        Assert.Equal(TimeSpan.Zero, hold.Update("Spotify.exe", isShownPlaying: false, Start));
        Assert.Equal(TimeSpan.Zero, hold.Update("Spotify.exe", isShownPlaying: false, Start.AddSeconds(1)));
    }

    [Fact]
    public void A_new_app_on_the_island_starts_fresh()
    {
        PauseHold hold = new();
        hold.Update("Spotify.exe", isShownPlaying: true, Start);
        hold.Update("Spotify.exe", isShownPlaying: false, Start);

        Assert.Equal(TimeSpan.Zero, hold.Update("Brave", isShownPlaying: true, Start.AddSeconds(1)));
        Assert.Equal(TimeSpan.FromSeconds(3), hold.Update("Brave", isShownPlaying: false, Start.AddSeconds(2)));
    }

    [Fact]
    public void No_app_on_the_island_is_never_held()
    {
        Assert.Equal(TimeSpan.Zero, new PauseHold().Update(null, isShownPlaying: false, Start));
    }
}
