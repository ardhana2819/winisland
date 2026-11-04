// ColorEmojiText.cs: draws a line of text the way WPF lays it out (so emoji sequences such as skin
// tones, flags and families are already joined by the font), then replaces every glyph that has
// color layers with those layers, each in its own color. Other glyphs stay as WPF drew them.

using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace EmojiProbe;

internal static class ColorEmojiText
{
    public static DrawingGroup Draw(string text, Typeface typeface, double size, Brush foreground, ColorLayers layers, double pixelsPerDip)
    {
        // 1. Let WPF lay out and shape the text, with its fallback to Segoe UI Emoji for emoji.
        FormattedText formatted = new(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, size, foreground, pixelsPerDip);
        DrawingVisual visual = new();
        using (DrawingContext context = visual.RenderOpen())
        {
            context.DrawText(formatted, new Point(0, 0));
        }

        // 2. Walk what WPF drew and recolor the emoji glyphs.
        DrawingGroup result = new();
        using (DrawingContext context = result.Open())
        {
            Recolor(visual.Drawing, context, layers, ((SolidColorBrush)foreground).Color);
        }

        return result;
    }

    private static void Recolor(Drawing drawing, DrawingContext context, ColorLayers layers, Color textColor)
    {
        switch (drawing)
        {
            case DrawingGroup group:
                foreach (Drawing child in group.Children)
                {
                    Recolor(child, context, layers, textColor);
                }

                break;
            case GlyphRunDrawing glyphs:
                DrawGlyphRun(glyphs, context, layers, textColor);
                break;
            default:
                context.DrawDrawing(drawing);
                break;
        }
    }

    private static void DrawGlyphRun(GlyphRunDrawing drawing, DrawingContext context, ColorLayers layers, Color textColor)
    {
        GlyphRun run = drawing.GlyphRun;
        bool isEmojiFont = run.GlyphTypeface.FamilyNames.Values.Any(name => name.Contains("Emoji", StringComparison.OrdinalIgnoreCase));
        if (!isEmojiFont || !run.GlyphIndices.Any(glyph => layers.TryGet(glyph, out _)))
        {
            context.DrawDrawing(drawing);
            return;
        }

        double x = run.BaselineOrigin.X;
        for (int i = 0; i < run.GlyphIndices.Count; i++)
        {
            ushort glyph = run.GlyphIndices[i];
            Point origin = new(x, run.BaselineOrigin.Y);
            if (run.GlyphOffsets is { Count: > 0 } offsets)
            {
                origin.Offset(offsets[i].X, -offsets[i].Y);
            }

            if (layers.TryGet(glyph, out (ushort Glyph, ushort Palette)[] glyphLayers))
            {
                foreach ((ushort layerGlyph, ushort palette) in glyphLayers)
                {
                    DrawOne(run, layerGlyph, origin, new SolidColorBrush(layers.ColorOf(palette, textColor)), context);
                }
            }
            else
            {
                DrawOne(run, glyph, origin, drawing.ForegroundBrush, context);
            }

            x += run.AdvanceWidths[i];
        }
    }

    private static void DrawOne(GlyphRun source, ushort glyph, Point origin, Brush brush, DrawingContext context)
    {
#pragma warning disable CS0618 // The pixelsPerDip overload below is the current one; this keeps the spike short.
        GlyphRun one = new(
            source.GlyphTypeface,
            source.BidiLevel,
            false,
            source.FontRenderingEmSize,
            (float)source.PixelsPerDip,
            [glyph],
            origin,
            [source.GlyphTypeface.AdvanceWidths[glyph] * source.FontRenderingEmSize],
            null,
            null,
            null,
            null,
            null,
            null);
#pragma warning restore CS0618
        context.DrawGlyphRun(brush, one);
    }
}
