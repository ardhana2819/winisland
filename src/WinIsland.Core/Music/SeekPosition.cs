// SeekPosition.cs: turns a click or drag on the progress bar into a song position (PRD FR-11).
// The fraction is where along the bar the pointer is, from 0 (start) to 1 (end).

namespace WinIsland.Core.Music;

public static class SeekPosition
{
    public static TimeSpan ForFraction(double fraction, TimeSpan duration)
    {
        if (double.IsNaN(fraction))
        {
            throw new ArgumentOutOfRangeException(nameof(fraction), fraction, "The position on the bar must be a number.");
        }

        if (duration <= TimeSpan.Zero)
        {
            return TimeSpan.Zero;
        }

        // A drag can leave the bar on either side; keep the result inside the song.
        double clamped = Math.Clamp(fraction, 0, 1);
        return TimeSpan.FromTicks((long)(duration.Ticks * clamped));
    }
}
