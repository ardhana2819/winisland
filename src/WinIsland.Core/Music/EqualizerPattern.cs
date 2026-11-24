// EqualizerPattern.cs: works out how the five equalizer bars move (PRD FR-48, DESIGN_SPEC
// section 15). Windows gives no sound levels, so each bar follows its own fixed loop of uneven
// heights that looks like music. The loops come from a fixed seed, so the equalizer looks the same
// every time, and they are made once, so playing music costs no extra work per frame.

namespace WinIsland.Core.Music;

public static class EqualizerPattern
{
    public const int BarCount = 5;
    public const int StepsPerLoop = 24;
    public const double LowestDips = 3;
    public const double HighestDips = 16;
    public const double ShortestStepSeconds = 0.12;
    public const double LongestStepSeconds = 0.2;

    // A random level is at least this much of the bar's reach, so a moving bar is rarely a dot.
    public const double LowestLevel = 0.15;

    // Two heights in a row differ by at least this much of the reach, so a bar never looks stuck.
    public const double SmallestLevelChange = 0.25;

    // How far each bar may reach, from the outer bars to the middle one: a sound-wave shape.
    private static readonly double[] BarReach = [0.55, 0.8, 1.0, 0.8, 0.55];

    // Heights while music plays with Windows animation effects off.
    private static readonly double[] StillHeights = [7, 12, 16, 10, 6];

    // Any fixed number works; it only has to stay the same so the look does not change.
    private const int Seed = 48;

    /// <summary>How far bar <paramref name="barIndex"/> may grow, from 0 to 1 of the full range.</summary>
    public static double ReachOf(int barIndex) => BarReach[CheckedIndex(barIndex)];

    /// <summary>The bar's height while music plays and animation effects are off.</summary>
    public static double StillHeightOf(int barIndex) => StillHeights[CheckedIndex(barIndex)];

    /// <summary>
    /// The loop for one bar. The bar moves through the steps in order and then starts again; it
    /// begins at the last step's height, so the loop joins up without a jump.
    /// </summary>
    public static IReadOnlyList<EqualizerStep> LoopOf(int barIndex)
    {
        double reach = ReachOf(barIndex);
        Random random = new(Seed + barIndex);
        EqualizerStep[] steps = new EqualizerStep[StepsPerLoop];
        double firstLevel = 0;
        double? previousLevel = null;

        for (int index = 0; index < StepsPerLoop; index++)
        {
            double level = NextLevel(random, previousLevel);
            // The last level must also differ enough from the first one, where the loop starts again.
            while (index == StepsPerLoop - 1 && Math.Abs(level - firstLevel) < SmallestLevelChange)
            {
                level = NextLevel(random, previousLevel);
            }

            if (index == 0)
            {
                firstLevel = level;
            }

            previousLevel = level;
            double seconds = ShortestStepSeconds + (random.NextDouble() * (LongestStepSeconds - ShortestStepSeconds));
            steps[index] = new EqualizerStep(LowestDips + ((HighestDips - LowestDips) * reach * level), seconds);
        }

        return steps;
    }

    private static double NextLevel(Random random, double? previousLevel)
    {
        while (true)
        {
            double level = LowestLevel + (random.NextDouble() * (1 - LowestLevel));
            if (previousLevel is not double previous || Math.Abs(level - previous) >= SmallestLevelChange)
            {
                return level;
            }
        }
    }

    private static int CheckedIndex(int barIndex)
    {
        if (barIndex is < 0 or >= BarCount)
        {
            throw new ArgumentOutOfRangeException(nameof(barIndex), barIndex, $"The equalizer has {BarCount} bars.");
        }

        return barIndex;
    }
}
