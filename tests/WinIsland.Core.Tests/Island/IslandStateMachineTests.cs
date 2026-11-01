// IslandStateMachineTests.cs: checks the island rules from PRD FR-2, FR-5, FR-6, FR-10 and sections 12 and 13.

using WinIsland.Core.Island;
using WinIsland.Core.Music;

namespace WinIsland.Core.Tests.Island;

public class IslandStateMachineTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 31, 20, 0, 0, TimeSpan.FromHours(7));

    [Fact]
    public void Starts_visible_compact_and_not_paused()
    {
        IslandStateMachine island = new();

        Assert.True(island.IsVisible);
        Assert.False(island.IsPaused);
        Assert.False(island.IsMusicPresent);
        Assert.Equal(IslandView.Compact, island.View);
        Assert.Null(island.NextTimeCheckAt);
    }

    // ---- Expanding (FR-2) ----

    [Fact]
    public void Expand_from_the_clock_without_music_opens_the_empty_view()
    {
        // v1.1 (FR-2, FR-24): replaces the v1.0 rule that expanding did nothing without music.
        IslandStateMachine island = new();
        ChangeCounter changes = CountChanges(island);

        island.ToggleExpanded(Now);

        Assert.Equal(IslandView.Expanded, island.View);
        Assert.Null(island.Music);
        Assert.False(island.IsMusicPresent);
        Assert.Equal(1, changes.Count);

        island.ToggleExpanded(Now);
        Assert.Equal(IslandView.Compact, island.View);
        Assert.Null(island.NextTimeCheckAt);
    }

    [Fact]
    public void Music_starting_while_the_empty_view_is_open_fills_it()
    {
        IslandStateMachine island = new();
        island.ToggleExpanded(Now);

        island.UpdateMusic(Track(PlaybackStatus.Playing), Now);

        Assert.Equal(IslandView.Expanded, island.View);
        Assert.True(island.IsMusicPresent);

        island.Collapse(Now);
        Assert.Equal(IslandView.Music, island.View);
    }

    [Fact]
    public void Expand_toggles_while_music_plays()
    {
        IslandStateMachine island = WithMusicPlaying();

        island.ToggleExpanded(Now);
        Assert.Equal(IslandView.Expanded, island.View);

        island.ToggleExpanded(Now);
        Assert.Equal(IslandView.Music, island.View);
    }

    [Fact]
    public void Collapse_closes_the_expanded_island_and_is_safe_when_already_closed()
    {
        IslandStateMachine island = WithMusicPlaying();
        island.ToggleExpanded(Now);

        island.Collapse(Now);
        ChangeCounter changes = CountChanges(island);
        island.Collapse(Now);

        Assert.Equal(IslandView.Music, island.View);
        Assert.Equal(0, changes.Count);
    }

    // ---- Hiding and pausing (FR-5, FR-6, section 12) ----

    [Fact]
    public void Hide_toggles_visibility_and_closes_the_expanded_island()
    {
        IslandStateMachine island = WithMusicPlaying();
        island.ToggleExpanded(Now);

        island.ToggleHidden(Now);
        Assert.False(island.IsVisible);
        Assert.Equal(IslandView.Compact, island.View);

        island.ToggleHidden(Now);
        Assert.True(island.IsVisible);
        Assert.Equal(IslandView.Music, island.View);
    }

    [Fact]
    public void Expand_does_nothing_while_hidden()
    {
        IslandStateMachine island = WithMusicPlaying();
        island.ToggleHidden(Now);

        island.ToggleExpanded(Now);
        island.ToggleHidden(Now);

        Assert.Equal(IslandView.Music, island.View);
    }

    [Fact]
    public void Pause_hides_the_island_for_one_hour()
    {
        IslandStateMachine island = WithMusicPlaying();
        island.ToggleExpanded(Now);

        island.Pause(Now);

        Assert.True(island.IsPaused);
        Assert.False(island.IsVisible);
        Assert.Equal(Now.AddHours(1), island.PausedUntil);
        Assert.Equal(Now.AddHours(1), island.NextTimeCheckAt);
        Assert.Equal(IslandView.Compact, island.View);
    }

    [Fact]
    public void Pause_stays_until_the_hour_is_over_then_ends_on_its_own()
    {
        IslandStateMachine island = new();
        island.Pause(Now);

        island.UpdateTime(Now.AddMinutes(59).AddSeconds(59));
        Assert.True(island.IsPaused);

        island.UpdateTime(Now.AddHours(1));
        Assert.False(island.IsPaused);
        Assert.True(island.IsVisible);
        Assert.Null(island.NextTimeCheckAt);
    }

    [Fact]
    public void Resume_ends_the_pause_early_and_does_nothing_when_not_paused()
    {
        IslandStateMachine island = new();
        island.Pause(Now);

        island.Resume();
        Assert.False(island.IsPaused);
        Assert.True(island.IsVisible);

        ChangeCounter changes = CountChanges(island);
        island.Resume();
        Assert.Equal(0, changes.Count);
    }

    [Fact]
    public void Paused_wins_over_showing_and_hidden_state_returns_after_pause()
    {
        IslandStateMachine island = new();
        island.Pause(Now);

        island.ToggleHidden(Now);
        Assert.False(island.IsVisible);

        island.Resume();
        Assert.False(island.IsVisible);
        Assert.True(island.IsHiddenByUser);
    }

    [Fact]
    public void Expand_does_nothing_while_paused()
    {
        IslandStateMachine island = WithMusicPlaying();
        island.Pause(Now);

        island.ToggleExpanded(Now);
        island.Resume();

        Assert.Equal(IslandView.Music, island.View);
    }

    // ---- Music (FR-7, FR-10, section 13) ----

    [Fact]
    public void Playing_music_shows_the_music_pill()
    {
        IslandStateMachine island = new();

        island.UpdateMusic(Track(PlaybackStatus.Playing), Now);

        Assert.True(island.IsMusicPresent);
        Assert.Equal(IslandView.Music, island.View);
        Assert.Equal("Like a Stone", island.Music?.Title);
    }

    [Fact]
    public void Paused_music_shows_for_5_seconds_then_the_clock()
    {
        IslandStateMachine island = WithMusicPlaying();

        island.UpdateMusic(Track(PlaybackStatus.Paused), Now);
        Assert.Equal(IslandView.Music, island.View);
        Assert.Equal(Now.AddSeconds(5), island.NextTimeCheckAt);

        island.UpdateTime(Now.AddSeconds(4.9));
        Assert.Equal(IslandView.Music, island.View);

        island.UpdateTime(Now.AddSeconds(5));
        Assert.Equal(IslandView.Compact, island.View);
        Assert.False(island.IsMusicPresent);
        Assert.Null(island.NextTimeCheckAt);
    }

    [Fact]
    public void Music_that_appears_already_paused_also_shows_for_5_seconds()
    {
        IslandStateMachine island = new();

        island.UpdateMusic(Track(PlaybackStatus.Paused), Now);
        Assert.Equal(IslandView.Music, island.View);

        island.UpdateTime(Now.AddSeconds(5));
        Assert.Equal(IslandView.Compact, island.View);
    }

    [Fact]
    public void Paused_music_stays_while_expanded_and_the_5_seconds_start_on_close()
    {
        IslandStateMachine island = WithMusicPlaying();
        island.ToggleExpanded(Now);

        // Pausing from the expanded island must not take the play button away.
        island.UpdateMusic(Track(PlaybackStatus.Paused), Now);
        island.UpdateTime(Now.AddMinutes(1));
        Assert.Equal(IslandView.Expanded, island.View);
        Assert.Null(island.NextTimeCheckAt);

        DateTimeOffset closedAt = Now.AddMinutes(2);
        island.Collapse(closedAt);
        Assert.Equal(IslandView.Music, island.View);
        Assert.Equal(closedAt.AddSeconds(5), island.NextTimeCheckAt);

        island.UpdateTime(closedAt.AddSeconds(5));
        Assert.Equal(IslandView.Compact, island.View);
    }

    [Fact]
    public void Expanding_paused_music_stops_the_countdown()
    {
        IslandStateMachine island = WithMusicPlaying();
        island.UpdateMusic(Track(PlaybackStatus.Paused), Now);

        island.ToggleExpanded(Now.AddSeconds(2));
        island.UpdateTime(Now.AddSeconds(10));

        Assert.Equal(IslandView.Expanded, island.View);
    }

    [Fact]
    public void Paused_music_after_the_5_seconds_expands_again_with_the_paused_song()
    {
        // v1.1 (FR-23): replaces the v1.0 rule that paused music could not be expanded once the clock was back.
        IslandStateMachine island = WithMusicPlaying();
        island.UpdateMusic(Track(PlaybackStatus.Paused), Now);
        island.UpdateTime(Now.AddSeconds(5));
        Assert.Equal(IslandView.Compact, island.View);

        island.ToggleExpanded(Now.AddSeconds(6));

        Assert.Equal(IslandView.Expanded, island.View);
        Assert.Equal("Like a Stone", island.Music?.Title);
        Assert.False(island.Music?.IsPlaying);
        Assert.Null(island.NextTimeCheckAt);

        // Closing shows the paused song for 5 seconds again, then the clock (section 13).
        DateTimeOffset closedAt = Now.AddSeconds(20);
        island.Collapse(closedAt);
        Assert.Equal(IslandView.Music, island.View);
        Assert.Equal(closedAt.AddSeconds(5), island.NextTimeCheckAt);

        island.UpdateTime(closedAt.AddSeconds(5));
        Assert.Equal(IslandView.Compact, island.View);
    }

    [Fact]
    public void Playing_from_the_reopened_paused_song_keeps_the_music()
    {
        IslandStateMachine island = WithMusicPlaying();
        island.UpdateMusic(Track(PlaybackStatus.Paused), Now);
        island.UpdateTime(Now.AddSeconds(5));
        island.ToggleExpanded(Now.AddSeconds(6));

        island.UpdateMusic(Track(PlaybackStatus.Playing), Now.AddSeconds(7));
        island.Collapse(Now.AddSeconds(8));

        Assert.Equal(IslandView.Music, island.View);
        Assert.Null(island.NextTimeCheckAt);
    }

    [Fact]
    public void Playing_again_brings_the_music_back_and_stops_the_countdown()
    {
        IslandStateMachine island = WithMusicPlaying();
        island.UpdateMusic(Track(PlaybackStatus.Paused), Now);
        island.UpdateTime(Now.AddSeconds(5));

        island.UpdateMusic(Track(PlaybackStatus.Playing), Now.AddSeconds(30));

        Assert.Equal(IslandView.Music, island.View);
        Assert.Null(island.NextTimeCheckAt);
    }

    [Fact]
    public void Music_session_closing_returns_to_the_clock()
    {
        IslandStateMachine island = WithMusicPlaying();

        island.UpdateMusic(null, Now);

        Assert.False(island.IsMusicPresent);
        Assert.Null(island.Music);
        Assert.Equal(IslandView.Compact, island.View);
    }

    [Fact]
    public void Music_session_closing_while_expanded_keeps_the_island_open_on_the_empty_view()
    {
        // v1.1 (FR-24, section 13): replaces the v1.0 rule that closed the expanded island.
        IslandStateMachine island = WithMusicPlaying();
        island.ToggleExpanded(Now);

        island.UpdateMusic(null, Now);

        Assert.Equal(IslandView.Expanded, island.View);
        Assert.Null(island.Music);
        Assert.False(island.IsMusicPresent);

        island.Collapse(Now);
        Assert.Equal(IslandView.Compact, island.View);
        Assert.Null(island.NextTimeCheckAt);
    }

    [Fact]
    public void Next_time_check_is_the_earlier_of_pause_end_and_paused_music_end()
    {
        IslandStateMachine island = WithMusicPlaying();
        island.UpdateMusic(Track(PlaybackStatus.Paused), Now);

        island.Pause(Now);

        Assert.Equal(Now.AddSeconds(5), island.NextTimeCheckAt);
    }

    [Fact]
    public void A_new_report_with_the_same_values_raises_no_change()
    {
        IslandStateMachine island = WithMusicPlaying();
        ChangeCounter changes = CountChanges(island);

        island.UpdateMusic(Track(PlaybackStatus.Playing), Now);

        Assert.Equal(0, changes.Count);
    }

    // ---- Hotkey conflict banner (FR-5) ----

    [Fact]
    public void Hotkey_conflict_banner_shows_and_dismisses()
    {
        IslandStateMachine island = new();

        island.ShowHotkeyConflict("Alt+X is used by another app");
        Assert.Equal(IslandView.HotkeyConflict, island.View);
        Assert.Equal("Alt+X is used by another app", island.HotkeyConflictText);

        island.DismissHotkeyConflict();
        Assert.Equal(IslandView.Compact, island.View);
        Assert.Null(island.HotkeyConflictText);
    }

    [Fact]
    public void Hotkey_conflict_banner_wins_over_expanded_and_returns_to_it()
    {
        IslandStateMachine island = WithMusicPlaying();
        island.ToggleExpanded(Now);

        island.ShowHotkeyConflict("Ctrl+Alt+I is used by another app");
        Assert.Equal(IslandView.HotkeyConflict, island.View);

        island.DismissHotkeyConflict();
        Assert.Equal(IslandView.Expanded, island.View);
    }

    [Fact]
    public void Hotkey_conflict_banner_is_not_visible_while_hidden()
    {
        IslandStateMachine island = new();
        island.ShowHotkeyConflict("Alt+X is used by another app");

        island.ToggleHidden(Now);

        Assert.False(island.IsVisible);
        Assert.Equal(IslandView.Compact, island.View);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Hotkey_conflict_banner_needs_a_message(string message)
    {
        IslandStateMachine island = new();

        Assert.Throws<ArgumentException>(() => island.ShowHotkeyConflict(message));
    }

    [Fact]
    public void Dismissing_when_no_conflict_banner_shows_changes_nothing()
    {
        IslandStateMachine island = new();
        ChangeCounter changes = CountChanges(island);

        island.DismissHotkeyConflict();

        Assert.Equal(0, changes.Count);
    }

    // ---- Change notifications ----

    [Fact]
    public void Changed_is_raised_for_each_real_change()
    {
        IslandStateMachine island = new();
        int changes = 0;
        island.Changed += (_, _) => changes++;

        island.UpdateMusic(Track(PlaybackStatus.Playing), Now);
        island.UpdateMusic(Track(PlaybackStatus.Playing), Now); // no change
        island.ToggleExpanded(Now);
        island.ToggleHidden(Now);
        island.Pause(Now);
        island.Resume();

        Assert.Equal(5, changes);
    }

    private static NowPlaying Track(PlaybackStatus status)
    {
        return new NowPlaying("Like a Stone", "Audioslave", status, TimeSpan.FromSeconds(285), TimeSpan.FromSeconds(147), Now, canSeek: true);
    }

    private static IslandStateMachine WithMusicPlaying()
    {
        IslandStateMachine island = new();
        island.UpdateMusic(Track(PlaybackStatus.Playing), Now);
        return island;
    }

    private static ChangeCounter CountChanges(IslandStateMachine island)
    {
        ChangeCounter counter = new();
        island.Changed += (_, _) => counter.Count++;
        return counter;
    }

    // A reference type, so the test sees changes counted after the helper returns.
    private sealed class ChangeCounter
    {
        public int Count { get; set; }
    }
}
