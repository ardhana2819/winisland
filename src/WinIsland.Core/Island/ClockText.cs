// ClockText.cs: turns a time into the text the idle island shows.
// PRD section 8: 24-hour format, hours and minutes only, no seconds and no AM/PM.

using System.Globalization;

namespace WinIsland.Core.Island;

public static class ClockText
{
    public static string Format(DateTime localTime)
    {
        // Invariant culture so the separator is always a colon, whatever the Windows region is.
        return localTime.ToString("HH:mm", CultureInfo.InvariantCulture);
    }
}
