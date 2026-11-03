// PrivacyIndicatorTests.cs: checks the camera and microphone dots' rules (PRD FR-40): camera wins
// over the microphone, and an app uses a device while it has started and not stopped.

using WinIsland.Core.Privacy;

namespace WinIsland.Core.Tests.Privacy;

public class PrivacyIndicatorTests
{
    [Theory]
    [InlineData(false, false, PrivacyIndicator.None)]
    [InlineData(false, true, PrivacyIndicator.Microphone)]
    [InlineData(true, false, PrivacyIndicator.Camera)]
    [InlineData(true, true, PrivacyIndicator.Camera)]
    public void The_camera_wins_over_the_microphone(bool camera, bool microphone, PrivacyIndicator expected)
    {
        Assert.Equal(expected, PrivacyIndicators.For(camera, microphone));
    }

    [Theory]
    [InlineData(134000000000000000L, 0L, true)]
    [InlineData(134000000000000000L, 134000000000050000L, false)]
    [InlineData(0L, 0L, false)]
    public void A_device_is_in_use_while_started_and_not_stopped(long start, long stop, bool inUse)
    {
        Assert.Equal(inUse, PrivacyIndicators.IsInUse(start, stop));
    }
}
