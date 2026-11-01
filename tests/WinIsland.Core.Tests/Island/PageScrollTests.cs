// PageScrollTests.cs: checks that scrolling switches pages one gesture at a time (FR-21,
// DESIGN_SPEC section 9): steps less than 300 ms apart belong to one gesture.

using WinIsland.Core.Island;

namespace WinIsland.Core.Tests.Island;

public class PageScrollTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 1, 20, 0, 0, TimeSpan.FromHours(7));

    [Fact]
    public void Scrolling_down_shows_notifications_and_up_shows_music()
    {
        PageScroll scroll = new();

        Assert.Equal(ExpandedPage.Notifications, scroll.Step(-120, Now));
        Assert.Equal(ExpandedPage.Music, scroll.Step(120, Now.AddSeconds(1)));
    }

    [Fact]
    public void A_touchpad_swipe_of_many_small_steps_counts_once()
    {
        PageScroll scroll = new();

        Assert.Equal(ExpandedPage.Notifications, scroll.Step(-8, Now));
        for (int step = 1; step <= 20; step++)
        {
            // A touchpad keeps sending steps, even in the other direction while the swipe settles.
            Assert.Null(scroll.Step(step % 2 == 0 ? -8 : 4, Now.AddMilliseconds(step * 16)));
        }
    }

    [Fact]
    public void Separate_wheel_notches_each_count()
    {
        PageScroll scroll = new();

        Assert.Equal(ExpandedPage.Notifications, scroll.Step(-120, Now));
        Assert.Equal(ExpandedPage.Music, scroll.Step(120, Now.AddMilliseconds(300)));
    }

    [Fact]
    public void A_zero_step_changes_nothing()
    {
        PageScroll scroll = new();

        Assert.Null(scroll.Step(0, Now));
    }
}
