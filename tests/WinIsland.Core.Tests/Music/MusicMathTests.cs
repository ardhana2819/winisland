// MusicMathTests.cs: checks the music helpers: position estimate (FR-9), time labels, seek math (FR-11),
// and the NowPlaying input checks.

using WinIsland.Core.Music;

namespace WinIsland.Core.Tests.Music;

public class MusicMathTests
{
    private static readonly DateTimeOffset ReportedAt = new(2026, 10, 31, 20, 0, 0, TimeSpan.FromHours(7));

    [Fact]
    public void Playing_position_moves_on_with_the_time_since_the_last_report()
    {
        NowPlaying music = Track(PlaybackStatus.Playing, position: 60, duration: 200);

        Assert.Equal(TimeSpan.FromSeconds(63.5), PlaybackPosition.Estimate(music, ReportedAt.AddSeconds(3.5)));
    }

    [Fact]
    public void Paused_position_stays_where_it_was()
    {
        NowPlaying music = Track(PlaybackStatus.Paused, position: 60, duration: 200);

        Assert.Equal(TimeSpan.FromSeconds(60), PlaybackPosition.Estimate(music, ReportedAt.AddMinutes(5)));
    }

    [Fact]
    public void Position_never_runs_past_the_end_of_the_song()
    {
        NowPlaying music = Track(PlaybackStatus.Playing, position: 198, duration: 200);

        Assert.Equal(TimeSpan.FromSeconds(200), PlaybackPosition.Estimate(music, ReportedAt.AddSeconds(30)));
    }

    [Fact]
    public void A_report_from_the_future_does_not_move_the_position_backwards()
    {
        NowPlaying music = Track(PlaybackStatus.Playing, position: 60, duration: 200);

        Assert.Equal(TimeSpan.FromSeconds(60), PlaybackPosition.Estimate(music, ReportedAt.AddSeconds(-2)));
    }

    [Fact]
    public void Fraction_is_position_over_length_and_zero_for_unknown_length()
    {
        Assert.Equal(0.5, PlaybackPosition.Fraction(Track(PlaybackStatus.Paused, 100, 200), ReportedAt), precision: 6);
        Assert.Equal(0, PlaybackPosition.Fraction(Track(PlaybackStatus.Paused, 100, 0), ReportedAt));
    }

    [Theory]
    [InlineData(0, "0:00")]
    [InlineData(84, "1:24")]
    [InlineData(221, "3:41")]
    [InlineData(599, "9:59")]
    [InlineData(3725, "1:02:05")]
    [InlineData(-5, "0:00")]
    public void Time_labels_look_like_the_prototype(int seconds, string expected)
    {
        Assert.Equal(expected, TimeLabel.Format(TimeSpan.FromSeconds(seconds)));
    }

    [Theory]
    [InlineData(0.0, 0)]
    [InlineData(0.25, 50)]
    [InlineData(1.0, 200)]
    [InlineData(-0.3, 0)]
    [InlineData(1.4, 200)]
    public void Seek_turns_a_place_on_the_bar_into_a_position_inside_the_song(double fraction, int expectedSeconds)
    {
        Assert.Equal(TimeSpan.FromSeconds(expectedSeconds), SeekPosition.ForFraction(fraction, TimeSpan.FromSeconds(200)));
    }

    [Fact]
    public void Seek_with_unknown_length_goes_to_the_start_and_rejects_nonsense()
    {
        Assert.Equal(TimeSpan.Zero, SeekPosition.ForFraction(0.5, TimeSpan.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(() => SeekPosition.ForFraction(double.NaN, TimeSpan.FromSeconds(200)));
    }

    [Fact]
    public void Now_playing_rejects_a_negative_length_and_cleans_up_missing_text()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new NowPlaying("t", "a", PlaybackStatus.Playing, TimeSpan.FromSeconds(-1), TimeSpan.Zero, ReportedAt, canSeek: true));

        NowPlaying music = new(null!, null!, PlaybackStatus.Paused, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(-3), ReportedAt, canSeek: false);
        Assert.Equal(string.Empty, music.Title);
        Assert.Equal(string.Empty, music.Artist);
        Assert.Equal(TimeSpan.Zero, music.Position);
    }

    private static NowPlaying Track(PlaybackStatus status, int position, int duration)
    {
        return new NowPlaying("Song", "Artist", status, TimeSpan.FromSeconds(duration), TimeSpan.FromSeconds(position), ReportedAt, canSeek: true);
    }
}
