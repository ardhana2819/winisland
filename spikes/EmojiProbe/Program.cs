// Program.cs: renders sample notification lines with and without color emoji to PNG files next to
// the program, and prints how long the font's color tables took to read and how much memory that
// used. Run: dotnet run --project spikes/EmojiProbe -c Release -- <output folder>

using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EmojiProbe;

internal static class Program
{
    private static readonly string[] Samples =
    [
        // Written as escapes, so no editor or shell can garble them.
        "Mantap 😂😂 nanti malem jadi? 👍",
        "Love you ❤️ 🔥 🎉 ✨ 🙏",
        "Skin tones 👍🏽 👋🏿 - flags 🇮🇩 🇯🇵 - family 👨‍👩‍👧",
        "Keycap 1️⃣ - newer 🫠 🫶 🥹 - text ☺ ✔",
    ];

    [STAThread]
    private static void Main(string[] args)
    {
        string outFolder = args.Length > 0 ? args[0] : AppContext.BaseDirectory;
        Directory.CreateDirectory(outFolder);
        string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "seguiemj.ttf");

        long before = GC.GetTotalMemory(forceFullCollection: true);
        Stopwatch watch = Stopwatch.StartNew();
        ColorLayers layers = ColorLayers.Read(fontPath);
        watch.Stop();
        long after = GC.GetTotalMemory(forceFullCollection: true);
        Console.WriteLine($"Read {layers.GlyphCount} color glyphs in {watch.ElapsedMilliseconds} ms; kept {(after - before) / 1024} KB");

        Typeface typeface = new(new FontFamily("Segoe UI Variable Text, Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        for (int i = 0; i < Samples.Length; i++)
        {
            Stopwatch draw = Stopwatch.StartNew();
            DrawingGroup color = ColorEmojiText.Draw(Samples[i], typeface, 26, Brushes.White, layers, 1.0);
            draw.Stop();
            Save(color, Samples[i], typeface, Path.Combine(outFolder, $"sample{i + 1}.png"));
            Console.WriteLine($"Sample {i + 1}: drawn in {draw.Elapsed.TotalMilliseconds:0.0} ms");
        }
    }

    // Top: as WPF draws it today (outlines). Bottom: with the color layers.
    private static void Save(DrawingGroup color, string text, Typeface typeface, string path)
    {
        DrawingVisual plain = new();
        FormattedText formatted = new(text, System.Globalization.CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, 26, Brushes.White, 1.0);
        int width = (int)Math.Ceiling(Math.Max(formatted.WidthIncludingTrailingWhitespace, color.Bounds.Right)) + 24;
        int height = 100;
        using (DrawingContext context = plain.RenderOpen())
        {
            context.DrawRectangle(Brushes.Black, null, new Rect(0, 0, width, height));
            context.DrawText(formatted, new Point(12, 6));
            context.PushTransform(new TranslateTransform(12, 54));
            context.DrawDrawing(color);
            context.Pop();
        }

        RenderTargetBitmap bitmap = new(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(plain);
        PngBitmapEncoder encoder = new();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using FileStream file = File.Create(path);
        encoder.Save(file);
    }
}
