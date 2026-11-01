// RgbColor.cs: a plain red, green and blue color, so Core can work out the artwork colors
// (PRD FR-25) without any Windows drawing types.

namespace WinIsland.Core.Music;

public readonly record struct RgbColor(byte R, byte G, byte B)
{
    public static readonly RgbColor Black = new(0, 0, 0);
    public static readonly RgbColor White = new(255, 255, 255);

    /// <summary>For logs and test messages, for example #3F1216.</summary>
    public override string ToString() => $"#{R:X2}{G:X2}{B:X2}";
}
