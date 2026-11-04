// EmojiColorTable.cs: reads the simple color layers of an emoji font (PRD FR-47, spike E1): the
// version 0 part of the font's COLR table, which lists for each emoji glyph the glyphs to draw on
// top of each other and their palette colors, and the CPAL table, which holds those colors.
// Windows' Segoe UI Emoji keeps this simple version next to its newer gradient one, so the island
// can draw each emoji as a few flat colored shapes, with no library.
// Plain byte reading, no Windows API; the file is read by Platform.

using System.Buffers.Binary;

namespace WinIsland.Core.Text;

/// <summary>One layer of a color emoji: a glyph and its color as alpha, red, green, blue.</summary>
public readonly record struct EmojiLayer(ushort Glyph, byte Alpha, byte Red, byte Green, byte Blue, bool UsesTextColor);

public sealed class EmojiColorTable
{
    private const ushort TextColorIndex = 0xFFFF;

    private readonly Dictionary<ushort, EmojiLayer[]> _layers;

    private EmojiColorTable(Dictionary<ushort, EmojiLayer[]> layers) => _layers = layers;

    public int GlyphCount => _layers.Count;

    /// <summary>
    /// Reads the tables. colr may be only its version 0 part: the header, the base glyph records
    /// and the layer records. Throws FormatException when the bytes do not fit the layout.
    /// </summary>
    public static EmojiColorTable Parse(ReadOnlySpan<byte> colr, ReadOnlySpan<byte> cpal)
    {
        try
        {
            (byte A, byte R, byte G, byte B)[] palette = ReadPalette(cpal);
            ushort baseCount = BinaryPrimitives.ReadUInt16BigEndian(colr[2..]);
            int baseOffset = checked((int)BinaryPrimitives.ReadUInt32BigEndian(colr[4..]));
            int layerOffset = checked((int)BinaryPrimitives.ReadUInt32BigEndian(colr[8..]));

            Dictionary<ushort, EmojiLayer[]> layers = new(baseCount);
            for (int i = 0; i < baseCount; i++)
            {
                ReadOnlySpan<byte> record = colr.Slice(baseOffset + (i * 6), 6);
                ushort glyph = BinaryPrimitives.ReadUInt16BigEndian(record);
                ushort first = BinaryPrimitives.ReadUInt16BigEndian(record[2..]);
                ushort count = BinaryPrimitives.ReadUInt16BigEndian(record[4..]);
                EmojiLayer[] glyphLayers = new EmojiLayer[count];
                for (int j = 0; j < count; j++)
                {
                    ReadOnlySpan<byte> layer = colr.Slice(layerOffset + ((first + j) * 4), 4);
                    ushort layerGlyph = BinaryPrimitives.ReadUInt16BigEndian(layer);
                    ushort paletteIndex = BinaryPrimitives.ReadUInt16BigEndian(layer[2..]);
                    glyphLayers[j] = paletteIndex == TextColorIndex || paletteIndex >= palette.Length
                        ? new EmojiLayer(layerGlyph, 255, 0, 0, 0, UsesTextColor: true)
                        : new EmojiLayer(layerGlyph, palette[paletteIndex].A, palette[paletteIndex].R, palette[paletteIndex].G, palette[paletteIndex].B, UsesTextColor: false);
                }

                layers[glyph] = glyphLayers;
            }

            return new EmojiColorTable(layers);
        }
        catch (Exception error) when (error is ArgumentOutOfRangeException or OverflowException)
        {
            throw new FormatException("The emoji font's color tables do not have the expected layout.", error);
        }
    }

    /// <summary>How many bytes of the COLR table the version 0 part needs, from its header.</summary>
    public static int Version0Length(ReadOnlySpan<byte> colrHeader)
    {
        ushort baseCount = BinaryPrimitives.ReadUInt16BigEndian(colrHeader[2..]);
        uint baseOffset = BinaryPrimitives.ReadUInt32BigEndian(colrHeader[4..]);
        uint layerOffset = BinaryPrimitives.ReadUInt32BigEndian(colrHeader[8..]);
        ushort layerCount = BinaryPrimitives.ReadUInt16BigEndian(colrHeader[12..]);
        return checked((int)Math.Max(baseOffset + (baseCount * 6u), layerOffset + (layerCount * 4u)));
    }

    public bool TryGetLayers(ushort glyph, out EmojiLayer[] layers) => _layers.TryGetValue(glyph, out layers!);

    // CPAL version 0, palette 0: color records stored as blue, green, red, alpha.
    private static (byte, byte, byte, byte)[] ReadPalette(ReadOnlySpan<byte> cpal)
    {
        ushort colorCount = BinaryPrimitives.ReadUInt16BigEndian(cpal[2..]);
        int recordsOffset = checked((int)BinaryPrimitives.ReadUInt32BigEndian(cpal[8..]));
        ushort firstIndex = BinaryPrimitives.ReadUInt16BigEndian(cpal[12..]);
        (byte, byte, byte, byte)[] palette = new (byte, byte, byte, byte)[colorCount];
        for (int i = 0; i < colorCount; i++)
        {
            ReadOnlySpan<byte> color = cpal.Slice(recordsOffset + ((firstIndex + i) * 4), 4);
            palette[i] = (color[3], color[2], color[1], color[0]);
        }

        return palette;
    }
}
