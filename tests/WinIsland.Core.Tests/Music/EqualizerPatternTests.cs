// EqualizerPatternTests.cs: checks the equalizer bars' loops (FR-48, AC-48, DESIGN_SPEC
// section 15): heights stay in range, the middle bar reaches highest, steps are 0.12 to 0.2 s,
// bars never look stuck, the loops are the same every time and differ between bars.

using WinIsland.Core.Music;

namespace WinIsland.Core.Tests.Music;

public class EqualizerPatternTests
{
    public static TheoryData<int> Bars => [0, 1, 2, 3, 4];

    [Theory]
    [MemberData(nameof(Bars))]
    public void Every_height_stays_between_a_dot_and_the_bar_reach(int bar)
    {
        double highest = EqualizerPattern.LowestDips + ((EqualizerPattern.HighestDips - EqualizerPattern.LowestDips) * EqualizerPattern.ReachOf(bar));

        foreach (EqualizerStep step in EqualizerPattern.LoopOf(bar))
        {
            Assert.InRange(step.HeightDips, EqualizerPattern.LowestDips, highest);
        }
    }

    [Theory]
    [MemberData(nameof(Bars))]
    public void Every_step_lasts_between_0_12_and_0_2_seconds(int bar)
    {
        IReadOnlyList<EqualizerStep> loop = EqualizerPattern.LoopOf(bar);

        Assert.Equal(EqualizerPattern.StepsPerLoop, loop.Count);
        Assert.All(loop, step => Assert.InRange(step.Seconds, EqualizerPattern.ShortestStepSeconds, EqualizerPattern.LongestStepSeconds));
    }

    [Theory]
    [MemberData(nameof(Bars))]
    public void Two_heights_in_a_row_differ_enough_also_where_the_loop_starts_again(int bar)
    {
        IReadOnlyList<EqualizerStep> loop = EqualizerPattern.LoopOf(bar);
        double smallestChange = (EqualizerPattern.HighestDips - EqualizerPattern.LowestDips) * EqualizerPattern.ReachOf(bar) * EqualizerPattern.SmallestLevelChange;

        for (int index = 0; index < loop.Count; index++)
        {
            double previous = loop[(index + loop.Count - 1) % loop.Count].HeightDips;
            Assert.True(Math.Abs(loop[index].HeightDips - previous) >= smallestChange - 1e-9, $"Bar {bar}, step {index}");
        }
    }

    [Fact]
    public void The_middle_bar_reaches_highest_and_the_outer_bars_lowest()
    {
        Assert.Equal(1.0, EqualizerPattern.ReachOf(2));
        Assert.True(EqualizerPattern.ReachOf(1) > EqualizerPattern.ReachOf(0));
        Assert.Equal(EqualizerPattern.ReachOf(0), EqualizerPattern.ReachOf(4));
        Assert.Equal(EqualizerPattern.ReachOf(1), EqualizerPattern.ReachOf(3));
    }

    [Fact]
    public void The_loops_are_the_same_every_time()
    {
        Assert.Equal(EqualizerPattern.LoopOf(2), EqualizerPattern.LoopOf(2));
    }

    [Fact]
    public void The_bars_have_different_loops()
    {
        Assert.NotEqual(EqualizerPattern.LoopOf(1), EqualizerPattern.LoopOf(3));
        Assert.NotEqual(EqualizerPattern.LoopOf(1).Sum(step => step.Seconds), EqualizerPattern.LoopOf(3).Sum(step => step.Seconds));
    }

    [Fact]
    public void Still_heights_match_the_design()
    {
        double[] heights = [.. Enumerable.Range(0, EqualizerPattern.BarCount).Select(EqualizerPattern.StillHeightOf)];

        Assert.Equal([7, 12, 16, 10, 6], heights);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(5)]
    public void A_bar_outside_the_five_is_refused(int bar)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => EqualizerPattern.LoopOf(bar));
    }
}
