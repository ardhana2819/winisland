// TimeLabel.cs: formats song times for the expanded island, like the prototype: "1:24", "3:41".
// Songs of an hour or more get hours too: "1:02:05".

using System.Globalization;

namespace WinIsland.Core.Music;

public static class TimeLabel
{
    public static string Format(TimeSpan time)
    {
        if (time < TimeSpan.Zero)
        {
            time = TimeSpan.Zero;
        }

        int totalHours = (int)time.TotalHours;
        return totalHours > 0
            ? string.Format(CultureInfo.InvariantCulture, "{0}:{1:00}:{2:00}", totalHours, time.Minutes, time.Seconds)
            : string.Format(CultureInfo.InvariantCulture, "{0}:{1:00}", time.Minutes, time.Seconds);
    }
}
