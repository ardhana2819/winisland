// PlaybackPosition.cs: estimates where the song is now (PRD FR-9).
// Windows only reports the position now and then, so between reports the island moves the
// progress bar on by the time that has passed, as long as the music is playing.

namespace WinIsland.Core.Music;

public static class PlaybackPosition
{
    public static TimeSpan Estimate(NowPlaying music, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(music);

        TimeSpan position = music.Position;
        if (music.IsPlaying && now > music.PositionUpdatedAt)
        {
            position += now - music.PositionUpdatedAt;
        }

        // Never run past the end, even if the next report is late.
        if (music.Duration > TimeSpan.Zero && position > music.Duration)
        {
            return music.Duration;
        }

        return position;
    }

    /// <summary>How far through the song, from 0 to 1, for the progress bar.</summary>
    public static double Fraction(NowPlaying music, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(music);

        if (music.Duration <= TimeSpan.Zero)
        {
            return 0;
        }

        return Estimate(music, now) / music.Duration;
    }
}
