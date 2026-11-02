// IslandScaleTests.cs: checks the display scale rule from PRD FR-1 (width / 2560, limited to 0.85 to 1.25)
// and the size setting on top of it (FR-32: 0.85, 1, 1.15, 1.3).

using WinIsland.Core.Island;

namespace WinIsland.Core.Tests.Island;

public class IslandScaleTests
{
    [Theory]
    [InlineData(2560, 1.0)] // 27 inch 1440p, the design size
    [InlineData(3072, 1.2)] // 32 inch 4K at 125 %
    [InlineData(2304, 0.9)]
    public void Scale_follows_display_width_inside_the_limits(double widthDips, double expected)
    {
        Assert.Equal(expected, IslandScale.ForDisplayWidth(widthDips), precision: 6);
    }

    [Theory]
    [InlineData(1920, 0.85)] // 24 inch 1080p would be 0.75, limited to the minimum
    [InlineData(1280, 0.85)]
    [InlineData(3840, 1.25)] // 4K at 100 % would be 1.5, limited to the maximum
    public void Scale_is_limited_to_the_allowed_range(double widthDips, double expected)
    {
        Assert.Equal(expected, IslandScale.ForDisplayWidth(widthDips), precision: 6);
    }

    [Theory]
    [InlineData(IslandSize.Small, 0.85)]
    [InlineData(IslandSize.Medium, 1.0)]
    [InlineData(IslandSize.Large, 1.15)]
    [InlineData(IslandSize.ExtraLarge, 1.3)]
    public void Each_size_has_its_factor(IslandSize size, double expected)
    {
        Assert.Equal(expected, IslandScale.FactorOf(size), precision: 6);
    }

    [Theory]
    [InlineData(2560, IslandSize.Medium, 1.0)] // Medium is today's island
    [InlineData(2560, IslandSize.ExtraLarge, 1.3)] // 546 x 247 expanded
    [InlineData(3840, IslandSize.ExtraLarge, 1.625)] // the display limit 1.25 times 1.3
    [InlineData(1920, IslandSize.Small, 0.7225)] // the display limit 0.85 times 0.85
    public void The_size_multiplies_the_display_scale(double widthDips, IslandSize size, double expected)
    {
        Assert.Equal(expected, IslandScale.ForDisplayWidth(widthDips, size), precision: 6);
    }

    [Fact]
    public void An_unknown_size_is_rejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => IslandScale.FactorOf((IslandSize)99));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Invalid_width_is_rejected(double widthDips)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => IslandScale.ForDisplayWidth(widthDips));
    }
}
