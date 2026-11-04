// EmojiColorTableTests.cs: checks that the emoji font's simple color layers are read correctly
// (PRD FR-47) from small hand-made tables with the same layout as Segoe UI Emoji's.

using System.Buffers.Binary;

using WinIsland.Core.Text;

namespace WinIsland.Core.Tests.Text;

public class EmojiColorTableTests
{
    [Fact]
    public void A_glyph_gets_its_layers_with_their_colors()
    {
        EmojiColorTable table = EmojiColorTable.Parse(Colr(), Cpal());

        Assert.Equal(2, table.GlyphCount);
        Assert.True(table.TryGetLayers(100, out EmojiLayer[] layers));
        Assert.Equal(
            [
                new EmojiLayer(200, 255, 0xFF, 0xCC, 0x00, UsesTextColor: false),
                new EmojiLayer(201, 128, 0x11, 0x22, 0x33, UsesTextColor: false),
            ],
            layers);
    }

    [Fact]
    public void Palette_index_FFFF_means_the_text_color()
    {
        EmojiColorTable table = EmojiColorTable.Parse(Colr(), Cpal());

        Assert.True(table.TryGetLayers(101, out EmojiLayer[] layers));
        Assert.True(Assert.Single(layers).UsesTextColor);
    }

    [Fact]
    public void A_glyph_without_layers_is_not_found()
    {
        Assert.False(EmojiColorTable.Parse(Colr(), Cpal()).TryGetLayers(7, out _));
    }

    [Fact]
    public void The_version_0_length_comes_from_the_header()
    {
        // Header 14, 2 base records of 6 = 26; layers start at 26, 3 records of 4 = 38.
        Assert.Equal(38, EmojiColorTable.Version0Length(Colr()));
    }

    [Fact]
    public void Broken_tables_give_a_clear_error()
    {
        byte[] colr = Colr();
        BinaryPrimitives.WriteUInt32BigEndian(colr.AsSpan(8), 1000);

        Assert.Throws<FormatException>(() => EmojiColorTable.Parse(colr, Cpal()));
    }

    // COLR version 0: glyph 100 = layers 0 and 1, glyph 101 = layer 2 in the text color.
    private static byte[] Colr()
    {
        byte[] bytes = new byte[38];
        Span<byte> span = bytes;
        BinaryPrimitives.WriteUInt16BigEndian(span[2..], 2);
        BinaryPrimitives.WriteUInt32BigEndian(span[4..], 14);
        BinaryPrimitives.WriteUInt32BigEndian(span[8..], 26);
        BinaryPrimitives.WriteUInt16BigEndian(span[12..], 3);
        WriteBase(span[14..], glyph: 100, first: 0, count: 2);
        WriteBase(span[20..], glyph: 101, first: 2, count: 1);
        WriteLayer(span[26..], glyph: 200, palette: 0);
        WriteLayer(span[30..], glyph: 201, palette: 1);
        WriteLayer(span[34..], glyph: 202, palette: 0xFFFF);
        return bytes;
    }

    // CPAL version 0: one palette with two colors, stored as blue, green, red, alpha.
    private static byte[] Cpal()
    {
        byte[] bytes = new byte[14 + 8];
        Span<byte> span = bytes;
        BinaryPrimitives.WriteUInt16BigEndian(span[2..], 2);
        BinaryPrimitives.WriteUInt16BigEndian(span[4..], 1);
        BinaryPrimitives.WriteUInt16BigEndian(span[6..], 2);
        BinaryPrimitives.WriteUInt32BigEndian(span[8..], 14);
        BinaryPrimitives.WriteUInt16BigEndian(span[12..], 0);
        bytes[14] = 0x00; bytes[15] = 0xCC; bytes[16] = 0xFF; bytes[17] = 255;
        bytes[18] = 0x33; bytes[19] = 0x22; bytes[20] = 0x11; bytes[21] = 128;
        return bytes;
    }

    private static void WriteBase(Span<byte> at, ushort glyph, ushort first, ushort count)
    {
        BinaryPrimitives.WriteUInt16BigEndian(at, glyph);
        BinaryPrimitives.WriteUInt16BigEndian(at[2..], first);
        BinaryPrimitives.WriteUInt16BigEndian(at[4..], count);
    }

    private static void WriteLayer(Span<byte> at, ushort glyph, ushort palette)
    {
        BinaryPrimitives.WriteUInt16BigEndian(at, glyph);
        BinaryPrimitives.WriteUInt16BigEndian(at[2..], palette);
    }
}
