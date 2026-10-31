// IslandStateMachineTests.cs: checks the island rules from PRD FR-2, FR-5, FR-6 and section 12.

using WinIsland.Core.Island;

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
        Assert.Equal(IslandView.Compact, island.View);
    }

    [Fact]
    public void Expand_does_nothing_without_music()
    {
        IslandStateMachine island = new();
        ChangeCounter changes = CountChanges(island);

        island.ToggleExpanded();

        Assert.Equal(IslandView.Compact, island.View);
        Assert.Equal(0, changes.Count);
    }

    [Fact]
    public void Expand_toggles_while_music_is_present()
    {
        IslandStateMachine island = new();
        island.SetMusicPresent(true);

        island.ToggleExpanded();
        Assert.Equal(IslandView.Expanded, island.View);

        island.ToggleExpanded();
        Assert.Equal(IslandView.Compact, island.View);
    }

    [Fact]
    public void Collapse_closes_the_expanded_island_and_is_safe_when_already_compact()
    {
        IslandStateMachine island = new();
        island.SetMusicPresent(true);
        island.ToggleExpanded();

        island.Collapse();
        ChangeCounter changes = CountChanges(island);
        island.Collapse();

        Assert.Equal(IslandView.Compact, island.View);
        Assert.Equal(0, changes.Count);
    }

    [Fact]
    public void Music_stopping_closes_the_expanded_island()
    {
        IslandStateMachine island = new();
        island.SetMusicPresent(true);
        island.ToggleExpanded();

        island.SetMusicPresent(false);

        Assert.Equal(IslandView.Compact, island.View);
    }

    [Fact]
    public void Hide_toggles_visibility_and_closes_the_expanded_island()
    {
        IslandStateMachine island = new();
        island.SetMusicPresent(true);
        island.ToggleExpanded();

        island.ToggleHidden();
        Assert.False(island.IsVisible);
        Assert.Equal(IslandView.Compact, island.View);

        island.ToggleHidden();
        Assert.True(island.IsVisible);
        Assert.Equal(IslandView.Compact, island.View);
    }

    [Fact]
    public void Expand_does_nothing_while_hidden()
    {
        IslandStateMachine island = new();
        island.SetMusicPresent(true);
        island.ToggleHidden();

        island.ToggleExpanded();
        island.ToggleHidden();

        Assert.Equal(IslandView.Compact, island.View);
    }

    [Fact]
    public void Pause_hides_the_island_for_one_hour()
    {
        IslandStateMachine island = new();
        island.SetMusicPresent(true);
        island.ToggleExpanded();

        island.Pause(Now);

        Assert.True(island.IsPaused);
        Assert.False(island.IsVisible);
        Assert.Equal(Now.AddHours(1), island.PausedUntil);
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

        // Hiding during the pause is remembered for when the pause ends.
        island.ToggleHidden();
        Assert.False(island.IsVisible);

        island.Resume();
        Assert.False(island.IsVisible);
        Assert.True(island.IsHiddenByUser);
    }

    [Fact]
    public void Expand_does_nothing_while_paused()
    {
        IslandStateMachine island = new();
        island.SetMusicPresent(true);
        island.Pause(Now);

        island.ToggleExpanded();
        island.Resume();

        Assert.Equal(IslandView.Compact, island.View);
    }

    [Fact]
    public void Changed_is_raised_for_each_real_change()
    {
        IslandStateMachine island = new();
        int changes = 0;
        island.Changed += (_, _) => changes++;

        island.SetMusicPresent(true);
        island.SetMusicPresent(true); // no change
        island.ToggleExpanded();
        island.ToggleHidden();
        island.Pause(Now);
        island.Resume();

        Assert.Equal(5, changes);
    }

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
        IslandStateMachine island = new();
        island.SetMusicPresent(true);
        island.ToggleExpanded();

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

        island.ToggleHidden();

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
