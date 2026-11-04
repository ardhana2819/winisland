// EmojiSegmentsTests.cs: checks how text is split into plain text and emoji (PRD FR-47): joined
// emoji stay one emoji, text-style symbols stay text, and plain text is kept together.

using WinIsland.Core.Text;

namespace WinIsland.Core.Tests.Text;

public class EmojiSegmentsTests
{
    [Fact]
    public void Plain_text_is_one_segment()
    {
        IReadOnlyList<TextSegment> segments = EmojiSegments.Split("Nanti malem jadi?");

        Assert.Equal([new TextSegment("Nanti malem jadi?", IsEmoji: false)], segments);
    }

    [Fact]
    public void Emoji_and_text_alternate()
    {
        IReadOnlyList<TextSegment> segments = EmojiSegments.Split("Mantap \U0001F602\U0001F602 jadi? \U0001F44D");

        Assert.Equal(
            [
                new TextSegment("Mantap ", false),
                new TextSegment("\U0001F602", true),
                new TextSegment("\U0001F602", true),
                new TextSegment(" jadi? ", false),
                new TextSegment("\U0001F44D", true),
            ],
            segments);
    }

    [Theory]
    [InlineData("\U0001F44D\U0001F3FD")] // thumbs up, medium skin tone
    [InlineData("\U0001F468‍\U0001F469‍\U0001F467")] // family
    [InlineData("\U0001F1EE\U0001F1E9")] // flag of Indonesia
    [InlineData("1️⃣")] // keycap one
    [InlineData("❤️")] // red heart
    public void Joined_emoji_stay_one_emoji(string emoji)
    {
        IReadOnlyList<TextSegment> segments = EmojiSegments.Split($"a{emoji}b");

        Assert.Equal(3, segments.Count);
        Assert.Equal(new TextSegment(emoji, IsEmoji: true), segments[1]);
    }

    [Theory]
    [InlineData("✨", true)] // sparkles: emoji by default
    [InlineData("✅", true)] // check mark button
    [InlineData("☺", false)] // smiling face without U+FE0F: text by default
    [InlineData("✔", false)] // heavy check mark without U+FE0F
    [InlineData("✔️", true)]
    [InlineData("A", false)]
    [InlineData("1", false)]
    [InlineData("é", false)]
    public void Default_style_decides_symbols(string cluster, bool isEmoji)
    {
        Assert.Equal(isEmoji, EmojiSegments.IsEmoji(cluster));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Nothing_gives_no_segments(string? text)
    {
        Assert.Empty(EmojiSegments.Split(text));
    }
}
