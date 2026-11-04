// EmojiSegments.cs: splits text from another app into plain text and emoji (PRD FR-47), so the
// island can draw the emoji in color and the rest as normal text.
// It works on what a reader sees as one character (a grapheme cluster), so an emoji with a skin
// tone, a joined family, a flag or a keycap stays one emoji.
// An emoji is: anything from the emoji planes (U+1F000 and up), anything asking for emoji style
// (U+FE0F) or forming a keycap (U+20E3), and the symbols Windows shows as emoji by default. Symbols
// that are text by default, like ☺ without U+FE0F, stay text, as in Windows.

using System.Globalization;
using System.Text;

namespace WinIsland.Core.Text;

/// <summary>A run of plain text, or one emoji.</summary>
public sealed record TextSegment(string Text, bool IsEmoji);

public static class EmojiSegments
{
    private const int EmojiStyle = 0xFE0F;
    private const int Keycap = 0x20E3;
    private const int FirstEmojiPlane = 0x1F000;

    // Symbols below U+1F000 that are emoji by default (Unicode's Emoji_Presentation list).
    private static readonly (int First, int Last)[] DefaultEmojiSymbols =
    [
        (0x231A, 0x231B), (0x23E9, 0x23EC), (0x23F0, 0x23F0), (0x23F3, 0x23F3), (0x25FD, 0x25FE),
        (0x2614, 0x2615), (0x2648, 0x2653), (0x267F, 0x267F), (0x2693, 0x2693), (0x26A1, 0x26A1),
        (0x26AA, 0x26AB), (0x26BD, 0x26BE), (0x26C4, 0x26C5), (0x26CE, 0x26CE), (0x26D4, 0x26D4),
        (0x26EA, 0x26EA), (0x26F2, 0x26F3), (0x26F5, 0x26F5), (0x26FA, 0x26FA), (0x26FD, 0x26FD),
        (0x2705, 0x2705), (0x270A, 0x270B), (0x2728, 0x2728), (0x274C, 0x274C), (0x274E, 0x274E),
        (0x2753, 0x2755), (0x2757, 0x2757), (0x2795, 0x2797), (0x27B0, 0x27B0), (0x27BF, 0x27BF),
        (0x2B1B, 0x2B1C), (0x2B50, 0x2B50), (0x2B55, 0x2B55),
    ];

    public static IReadOnlyList<TextSegment> Split(string? text)
    {
        List<TextSegment> segments = [];
        if (string.IsNullOrEmpty(text))
        {
            return segments;
        }

        StringBuilder plain = new();
        TextElementEnumerator clusters = StringInfo.GetTextElementEnumerator(text);
        while (clusters.MoveNext())
        {
            string cluster = clusters.GetTextElement();
            if (!IsEmoji(cluster))
            {
                plain.Append(cluster);
                continue;
            }

            if (plain.Length > 0)
            {
                segments.Add(new TextSegment(plain.ToString(), IsEmoji: false));
                plain.Clear();
            }

            segments.Add(new TextSegment(cluster, IsEmoji: true));
        }

        if (plain.Length > 0)
        {
            segments.Add(new TextSegment(plain.ToString(), IsEmoji: false));
        }

        return segments;
    }

    /// <summary>Whether one cluster (what a reader sees as one character) is an emoji.</summary>
    public static bool IsEmoji(string cluster)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        bool first = true;
        foreach (Rune rune in cluster.EnumerateRunes())
        {
            int value = rune.Value;
            if (value is EmojiStyle or Keycap || value >= FirstEmojiPlane)
            {
                return true;
            }

            if (first && DefaultEmojiSymbols.Any(range => value >= range.First && value <= range.Last))
            {
                return true;
            }

            first = false;
        }

        return false;
    }
}
