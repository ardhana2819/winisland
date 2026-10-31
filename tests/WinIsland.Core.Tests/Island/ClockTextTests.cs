// ClockTextTests.cs: checks the idle clock format (24-hour, hours and minutes only).

using System.Globalization;

using WinIsland.Core.Island;

namespace WinIsland.Core.Tests.Island;

public class ClockTextTests
{
    [Theory]
    [InlineData(0, 0, 0, "00:00")]
    [InlineData(9, 5, 59, "09:05")]
    [InlineData(13, 45, 0, "13:45")]
    [InlineData(23, 59, 59, "23:59")]
    public void Shows_24_hour_time_without_seconds(int hour, int minute, int second, string expected)
    {
        DateTime time = new(2026, 10, 31, hour, minute, second);
        Assert.Equal(expected, ClockText.Format(time));
    }

    [Fact]
    public void Uses_a_colon_even_when_the_region_uses_another_separator()
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        try
        {
            // Finnish uses a dot as the time separator.
            CultureInfo.CurrentCulture = new CultureInfo("fi-FI");
            Assert.Equal("21:45", ClockText.Format(new DateTime(2026, 10, 31, 21, 45, 0)));
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}
