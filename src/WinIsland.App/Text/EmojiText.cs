// EmojiText.cs: puts text from another app into a TextBlock with its emoji in color (PRD FR-47,
// DESIGN_SPEC section 14): plain parts stay normal text, each emoji becomes a small color picture
// (ColorEmoji) on the text's baseline, and the line keeps its height. It stays one TextBlock, so
// the font, wrapping and, on Glass, the soft text shadow work as before. Text without emoji, or
// emoji the font has no colors for, is set exactly as before.
// Trimming: WPF's "…" does not hide pictures after the cut (an emoji stayed at the line's end), so
// a one-line block with trimming is cut here instead: whole pieces are kept while they fit, then
// "…". New text is always set whole first; only when layout then gives the block less width than
// the whole text needs is it cut, and it is worked out again whenever that width changes. So a
// block that is as wide as its text (the scrolling song title) is never cut. An emoji is never cut
// in half. cutToFit: false leaves trimming to WPF (the scrolling title's copies, which scroll instead
// of being cut). Blocks that are cut stretch across their column, as all of the island's do.
// A wrapping block with a height limit keeps WPF's trimming and is clipped at its limit.

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

using WinIsland.Core.Text;

namespace WinIsland.App.Text;

internal static class EmojiText
{
    private const string Ellipsis = "…";

    // The text each block shows, so a width change can lay it out again.
    private static readonly ConditionalWeakTable<TextBlock, Shown> ShownText = [];

    public static void Show(TextBlock block, string? text, bool cutToFit = true)
    {
        ArgumentNullException.ThrowIfNull(block);
        string value = text ?? string.Empty;
        IReadOnlyList<TextSegment> segments = EmojiSegments.Split(value);
        block.SizeChanged -= OnSizeChanged;
        if (!segments.Any(segment => segment.IsEmoji))
        {
            ShownText.Remove(block);
            block.Text = value;
            return;
        }

        Shown shown = new(segments);
        ShownText.AddOrUpdate(block, shown);
        Fill(block, shown, available: double.PositiveInfinity);
        if (cutToFit)
        {
            block.SizeChanged += OnSizeChanged;
        }

        // The pictures have no text, so screen readers get the whole line from the name.
        AutomationProperties.SetName(block, value);
    }

    private static void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.WidthChanged && sender is TextBlock block && ShownText.TryGetValue(block, out Shown? shown))
        {
            Fill(block, shown, block.ActualWidth - block.Padding.Left - block.Padding.Right);
        }
    }

    private static void Fill(TextBlock block, Shown shown, double available)
    {
        if (shown.FilledWidth == available)
        {
            return;
        }

        shown.FilledWidth = available;
        double pixelsPerDip = VisualTreeHelper.GetDpi(block).PixelsPerDip;
        Typeface typeface = new(block.FontFamily, block.FontStyle, block.FontWeight, block.FontStretch);
        double textAscent = Measure("x", typeface, block.FontSize, pixelsPerDip).Baseline;
        List<(string Text, EmojiPicture? Picture, double Width)> pieces = Pieces(shown.Segments, typeface, block.FontSize, pixelsPerDip);

        bool cutHere = block.TextTrimming != TextTrimming.None && block.TextWrapping == TextWrapping.NoWrap && available > 0
            && pieces.Sum(piece => piece.Width) > available;
        if (cutHere)
        {
            // One spare unit, because the pieces are measured one by one and kerning can add a little.
            pieces = Cut(pieces, available - Measure(Ellipsis, typeface, block.FontSize, pixelsPerDip).WidthIncludingTrailingWhitespace - 1);
            pieces.Add((Ellipsis, null, 0));
        }

        // A wrapping block with a height limit (a single notification's 3 lines) is trimmed by WPF on
        // its last line, but pictures of the hidden lines would still show below it.
        block.ClipToBounds = block.TextWrapping != TextWrapping.NoWrap && !double.IsInfinity(block.MaxHeight);

        block.Inlines.Clear();
        foreach ((string text, EmojiPicture? picture, _) in pieces)
        {
            block.Inlines.Add(picture is null ? new Run(text) : Picture(picture, textAscent));
        }
    }

    // Every cluster (what a reader sees as one character) on its own, so a cut can fall between any two.
    private static List<(string, EmojiPicture?, double)> Pieces(IReadOnlyList<TextSegment> segments, Typeface typeface, double size, double pixelsPerDip)
    {
        List<(string, EmojiPicture?, double)> pieces = [];
        foreach (TextSegment segment in segments)
        {
            EmojiPicture? picture = segment.IsEmoji ? ColorEmoji.For(segment.Text, size, pixelsPerDip) : null;
            if (picture is not null)
            {
                pieces.Add((segment.Text, picture, picture.Width));
                continue;
            }

            TextElementEnumerator clusters = StringInfo.GetTextElementEnumerator(segment.Text);
            while (clusters.MoveNext())
            {
                string cluster = clusters.GetTextElement();
                pieces.Add((cluster, null, Measure(cluster, typeface, size, pixelsPerDip).WidthIncludingTrailingWhitespace));
            }
        }

        return pieces;
    }

    private static List<(string, EmojiPicture?, double)> Cut(List<(string Text, EmojiPicture? Picture, double Width)> pieces, double room)
    {
        List<(string, EmojiPicture?, double)> kept = [];
        double used = 0;
        foreach ((string Text, EmojiPicture? Picture, double Width) piece in pieces)
        {
            if (used + piece.Width > room)
            {
                break;
            }

            kept.Add(piece);
            used += piece.Width;
        }

        // A cut line does not end in a space before the "…".
        while (kept.Count > 0 && kept[^1].Item1 is " ")
        {
            kept.RemoveAt(kept.Count - 1);
        }

        return kept;
    }

    // The holder is only as tall as the text's letters above the baseline, and its bottom sits on the
    // baseline, so the line keeps its height (a full emoji line box made it 3 to 4 taller). The
    // picture inside is placed so the emoji's own baseline is the holder's bottom; the part above or
    // below the holder is drawn but takes no room.
    private static InlineUIContainer Picture(EmojiPicture picture, double textAscent)
    {
        Image image = new()
        {
            Source = picture.Image,
            Width = picture.Width,
            Height = picture.Height,
            Stretch = Stretch.None,
        };
        Canvas.SetTop(image, textAscent - picture.Baseline);
        Canvas holder = new() { Width = picture.Width, Height = textAscent };
        holder.Children.Add(image);
        return new InlineUIContainer(holder) { BaselineAlignment = BaselineAlignment.Baseline };
    }

    private static FormattedText Measure(string text, Typeface typeface, double size, double pixelsPerDip) =>
        new(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, size, Brushes.White, pixelsPerDip);

    private sealed class Shown(IReadOnlyList<TextSegment> segments)
    {
        public IReadOnlyList<TextSegment> Segments { get; } = segments;

        public double FilledWidth { get; set; } = double.NaN;
    }
}
