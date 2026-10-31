// CubicBezierTests.cs: checks the timing curve math against known values of CSS cubic-bezier curves.

using WinIsland.Core.Animation;

namespace WinIsland.Core.Tests.Animation;

public class CubicBezierTests
{
    [Fact]
    public void Starts_at_0_and_ends_at_1()
    {
        Assert.Equal(0, CubicBezier.Spring.Evaluate(0));
        Assert.Equal(1, CubicBezier.Spring.Evaluate(1));
        Assert.Equal(0, CubicBezier.Ease.Evaluate(-0.5));
        Assert.Equal(1, CubicBezier.Ease.Evaluate(1.5));
    }

    [Theory]
    [InlineData(0.1)]
    [InlineData(0.5)]
    [InlineData(0.9)]
    public void A_straight_curve_returns_the_time_unchanged(double time)
    {
        CubicBezier linear = new(0, 0, 1, 1);
        Assert.Equal(time, linear.Evaluate(time), precision: 6);
    }

    [Fact]
    public void Ease_matches_the_CSS_ease_value_at_the_midpoint()
    {
        // Browsers give about 0.8024 for "ease" at 50 % of the time.
        Assert.Equal(0.8024, CubicBezier.Ease.Evaluate(0.5), precision: 3);
    }

    [Fact]
    public void Ease_only_moves_forward()
    {
        double previous = 0;
        for (int step = 1; step <= 100; step++)
        {
            double value = CubicBezier.Ease.Evaluate(step / 100.0);
            Assert.True(value >= previous, $"Ease went backwards at step {step}.");
            previous = value;
        }
    }

    [Fact]
    public void Spring_overshoots_past_1_and_settles_back()
    {
        double peak = 0;
        for (int step = 1; step < 100; step++)
        {
            peak = Math.Max(peak, CubicBezier.Spring.Evaluate(step / 100.0));
        }

        // The overshoot is what gives the island its spring feel (DESIGN_SPEC section 6).
        Assert.InRange(peak, 1.05, 1.2);
        Assert.Equal(1, CubicBezier.Spring.Evaluate(1));
    }

    [Theory]
    [InlineData(-0.1, 0, 1, 1)]
    [InlineData(0, 0, 1.1, 1)]
    public void Time_values_outside_0_to_1_are_rejected(double x1, double y1, double x2, double y2)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CubicBezier(x1, y1, x2, y2));
    }
}
