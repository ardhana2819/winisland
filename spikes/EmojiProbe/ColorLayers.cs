// ColorLayers.cs: reads the simple color layers of an emoji font (the COLR table's version 0
// records and the CPAL palette), so each emoji glyph can be drawn as its colored layers.
// Only the parts needed are read from the file, not the whole 13 MB font.

using System.Buffers.Binary;
using System.IO;
using System.Windows.Media;

namespace EmojiProbe;

internal sealed class ColorLayers
{
    private readonly Dictionary<ushort, (ushort Glyph, ushort Palette)[]> _layers = [];
    private readonly Color[] _palette;

    private ColorLayers(Color[] palette) => _palette = palette;

    public int GlyphCount => _layers.Count;

    public static ColorLayers Read(string fontPath)
    {
        using FileStream file = File.OpenRead(fontPath);
        Dictionary<string, (uint Offset, uint Length)> tables = ReadTableDirectory(file);
        byte[] colr = ReadTable(file, tables["COLR"]);
        byte[] cpal = ReadTable(file, tables["CPAL"]);

        // CPAL version 0: palette 0's colors, stored as blue, green, red, alpha.
        ushort colorCount = BinaryPrimitives.ReadUInt16BigEndian(cpal.AsSpan(6));
        uint recordsOffset = BinaryPrimitives.ReadUInt32BigEndian(cpal.AsSpan(8));
        ushort firstIndex = BinaryPrimitives.ReadUInt16BigEndian(cpal.AsSpan(12));
        Color[] palette = new Color[colorCount];
        for (int i = 0; i < colorCount; i++)
        {
            int at = (int)recordsOffset + ((firstIndex + i) * 4);
            palette[i] = Color.FromArgb(cpal[at + 3], cpal[at + 2], cpal[at + 1], cpal[at]);
        }

        // COLR version 0 part: base glyph records point at runs of layer records.
        ColorLayers result = new(palette);
        ushort baseCount = BinaryPrimitives.ReadUInt16BigEndian(colr.AsSpan(2));
        uint baseOffset = BinaryPrimitives.ReadUInt32BigEndian(colr.AsSpan(4));
        uint layerOffset = BinaryPrimitives.ReadUInt32BigEndian(colr.AsSpan(8));
        for (int i = 0; i < baseCount; i++)
        {
            int at = (int)baseOffset + (i * 6);
            ushort glyph = BinaryPrimitives.ReadUInt16BigEndian(colr.AsSpan(at));
            ushort first = BinaryPrimitives.ReadUInt16BigEndian(colr.AsSpan(at + 2));
            ushort count = BinaryPrimitives.ReadUInt16BigEndian(colr.AsSpan(at + 4));
            (ushort, ushort)[] layers = new (ushort, ushort)[count];
            for (int j = 0; j < count; j++)
            {
                int layer = (int)layerOffset + ((first + j) * 4);
                layers[j] = (BinaryPrimitives.ReadUInt16BigEndian(colr.AsSpan(layer)), BinaryPrimitives.ReadUInt16BigEndian(colr.AsSpan(layer + 2)));
            }

            result._layers[glyph] = layers;
        }

        return result;
    }

    public bool TryGet(ushort glyph, out (ushort Glyph, ushort Palette)[] layers) => _layers.TryGetValue(glyph, out layers!);

    /// <summary>A layer's color; 0xFFFF means the text color.</summary>
    public Color ColorOf(ushort paletteIndex, Color textColor) =>
        paletteIndex == 0xFFFF || paletteIndex >= _palette.Length ? textColor : _palette[paletteIndex];

    private static Dictionary<string, (uint, uint)> ReadTableDirectory(FileStream file)
    {
        byte[] header = new byte[12];
        file.ReadExactly(header);
        ushort count = BinaryPrimitives.ReadUInt16BigEndian(header.AsSpan(4));
        byte[] records = new byte[count * 16];
        file.ReadExactly(records);
        Dictionary<string, (uint, uint)> tables = [];
        for (int i = 0; i < count; i++)
        {
            ReadOnlySpan<byte> record = records.AsSpan(i * 16, 16);
            string tag = System.Text.Encoding.ASCII.GetString(record[..4]);
            tables[tag] = (BinaryPrimitives.ReadUInt32BigEndian(record[8..]), BinaryPrimitives.ReadUInt32BigEndian(record[12..]));
        }

        return tables;
    }

    private static byte[] ReadTable(FileStream file, (uint Offset, uint Length) table)
    {
        byte[] bytes = new byte[table.Length];
        file.Position = table.Offset;
        file.ReadExactly(bytes);
        return bytes;
    }
}
