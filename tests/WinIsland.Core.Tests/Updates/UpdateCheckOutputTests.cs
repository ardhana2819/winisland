// UpdateCheckOutputTests.cs: checks how the update script's quiet result line is read (PRD FR-46):
// only a newer version shows "Update available"; up to date, unclear or missing results show nothing.

using WinIsland.Core.Updates;

namespace WinIsland.Core.Tests.Updates;

public class UpdateCheckOutputTests
{
    [Fact]
    public void A_newer_release_gives_its_version()
    {
        string output = "Update WinIsland\r\n\r\nInstalled: 1.6.0-beta.1\r\nNewest release: 1.7.0-beta.1\r\nWINISLAND-UPDATE: available 1.7.0-beta.1\r\n";

        Assert.Equal("1.7.0-beta.1", UpdateCheckOutput.AvailableVersion(output));
    }

    [Theory]
    [InlineData("WINISLAND-UPDATE: latest")]
    [InlineData("WINISLAND-UPDATE: unknown")]
    [InlineData("Installed: 1.6.0-beta.1")]
    [InlineData("")]
    [InlineData(null)]
    public void Up_to_date_or_unclear_gives_nothing(string? output)
    {
        Assert.Null(UpdateCheckOutput.AvailableVersion(output));
    }

    [Theory]
    [InlineData("WINISLAND-UPDATE: available ")]
    [InlineData("WINISLAND-UPDATE: available v1.7.0")]
    [InlineData("WINISLAND-UPDATE: available 1.7.0 now")]
    [InlineData("WINISLAND-UPDATE: available 1.7.0;rm")]
    public void A_version_that_is_not_plain_gives_nothing(string output)
    {
        Assert.Null(UpdateCheckOutput.AvailableVersion(output));
    }

    [Fact]
    public void The_last_result_line_counts()
    {
        string output = "WINISLAND-UPDATE: available 1.7.0-beta.1\nWINISLAND-UPDATE: latest\n";

        Assert.Null(UpdateCheckOutput.AvailableVersion(output));
    }
}
