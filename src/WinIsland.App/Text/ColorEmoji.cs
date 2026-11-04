// ColorEmoji.cs: draws one emoji in color as a small picture (PRD FR-47, DESIGN_SPEC section 14).
// WPF draws emoji only as black and white outlines, so WPF lays the emoji out in Segoe UI Emoji
// (which joins skin tones, families and keycaps), and each glyph is then drawn again as the
// colored layers the font stores for it (EmojiColorTable, read once on the first emoji).
// The picture keeps the emoji's full line box, so it sits on the text's baseline like a letter.
// Pictures are cached per emoji and size; an emoji the font has no colors for gives null and
// stays plain text.

using System.Globalization;
using System.Windows;
using System.Windows.Media;

using WinIsland.Core.Text;
using WinIsland.Platform.Text;

namespace WinIsland.App.Text;

/// <summary>A color emoji picture and where its baseline is, from the top, in device-independent pixels.</summary>
internal sealed record EmojiPicture(ImageSource Image, double Width, double Height, double Baseline);

internal static class ColorEmoji
{
    private const int MaxCached = 300;

    private static readonly Lazy<EmojiColorTable?> Table = new(EmojiFont.ReadColorTable);
    private static readonly Typeface EmojiTypeface = new(new FontFamily(EmojiFont.FamilyName), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
    private static readonly Dictionary<(string, double), EmojiPicture?> Cache = [];

    /// <summary>The emoji in color at this font size, or null when it cannot be colored.</summary>
    public static EmojiPicture? For(string emoji, double fontSize, double pixelsPerDip)
    {
        if (Cache.TryGetValue((emoji, fontSize), out EmojiPicture? cached))
        {
            return cached;
        }

        EmojiPicture? picture = Table.Value is EmojiColorTable table ? Draw(emoji, fontSize, pixelsPerDip, table) : null;
        if (Cache.Count >= MaxCached)
        {
            Cache.Clear();
        }

        Cache[(emoji, fontSize)] = picture;
        return picture;
    }

    private static EmojiPicture? Draw(string emoji, double fontSize, double pixelsPerDip, EmojiColorTable table)
    {
        FormattedText laidOut = new(emoji, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, EmojiTypeface, fontSize, Brushes.White, pixelsPerDip);
        DrawingVisual visual = new();
        using (DrawingContext context = visual.RenderOpen())
        {
            context.DrawText(laidOut, new Point(0, 0));
        }

        double width = laidOut.WidthIncludingTrailingWhitespace;
        double height = laidOut.Height;
        DrawingGroup colored = new();
        bool hasColor = false;
        using (DrawingContext context = colored.Open())
        {
            // A clear box the size of the line, so the picture's size and baseline match the text's.
            context.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, width, height));
            foreach (GlyphRun run in GlyphRunsIn(visual.Drawing))
            {
                hasColor |= DrawInColor(run, table, context);
            }
        }

        if (!hasColor)
        {
            return null;
        }

        DrawingImage image = new(colored);
        image.Freeze();
        return new EmojiPicture(image, width, height, laidOut.Baseline);
    }

    private static IEnumerable<GlyphRun> GlyphRunsIn(Drawing? drawing)
    {
        if (drawing is GlyphRunDrawing glyphs)
        {
            yield return glyphs.GlyphRun;
        }
        else if (drawing is DrawingGroup group)
        {
            foreach (Drawing child in group.Children)
            {
                foreach (GlyphRun run in GlyphRunsIn(child))
                {
                    yield return run;
                }
            }
        }
    }

    // Each glyph with color layers is drawn as those layers; others as a white glyph.
    private static bool DrawInColor(GlyphRun run, EmojiColorTable table, DrawingContext context)
    {
        bool hasColor = false;
        double x = run.BaselineOrigin.X;
        for (int i = 0; i < run.GlyphIndices.Count; i++)
        {
            ushort glyph = run.GlyphIndices[i];
            Point origin = new(x, run.BaselineOrigin.Y);
            if (run.GlyphOffsets is { Count: > 0 } offsets)
            {
                origin.Offset(offsets[i].X, -offsets[i].Y);
            }

            if (table.TryGetLayers(glyph, out EmojiLayer[] layers))
            {
                hasColor = true;
                foreach (EmojiLayer layer in layers)
                {
                    Color color = layer.UsesTextColor ? Colors.White : Color.FromArgb(layer.Alpha, layer.Red, layer.Green, layer.Blue);
                    DrawGlyph(run, layer.Glyph, origin, new SolidColorBrush(color), context);
                }
            }
            else
            {
                DrawGlyph(run, glyph, origin, Brushes.White, context);
            }

            x += run.AdvanceWidths[i];
        }

        return hasColor;
    }

    private static void DrawGlyph(GlyphRun source, ushort glyph, Point origin, Brush brush, DrawingContext context)
    {
        GlyphRun one = new(
            source.GlyphTypeface,
            source.BidiLevel,
            false,
            source.FontRenderingEmSize,
            source.PixelsPerDip,
            [glyph],
            origin,
            [source.GlyphTypeface.AdvanceWidths[glyph] * source.FontRenderingEmSize],
            null,
            null,
            null,
            null,
            null,
            null);
        context.DrawGlyphRun(brush, one);
    }
}
