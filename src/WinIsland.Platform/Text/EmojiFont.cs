// EmojiFont.cs: reads the color tables of Windows' emoji font, Segoe UI Emoji (PRD FR-47), so the
// island can draw emoji in color. Only the parts needed are read from the 13 MB file: the table
// directory, the version 0 part of COLR (about 240 KB) and CPAL. Read once, on the first emoji.
// The file is only read; nothing else on the PC is touched. Without the font, or with a font of an
// unexpected layout, emoji simply stay as they are today.

using WinIsland.Core.Text;
using WinIsland.Platform.Diagnostics;

namespace WinIsland.Platform.Text;

public static class EmojiFont
{
    public const string FamilyName = "Segoe UI Emoji";
    private const string FileName = "seguiemj.ttf";
    private const int ColrHeaderLength = 14;

    /// <summary>The font's color layers, or null when they cannot be read.</summary>
    public static EmojiColorTable? ReadColorTable()
    {
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), FileName);
        try
        {
            using FileStream file = File.OpenRead(path);
            Dictionary<string, (uint Offset, uint Length)> tables = ReadTableDirectory(file);
            if (!tables.TryGetValue("COLR", out (uint Offset, uint Length) colr) || !tables.TryGetValue("CPAL", out (uint Offset, uint Length) cpal))
            {
                return null;
            }

            byte[] colrHeader = Read(file, colr.Offset, ColrHeaderLength);
            int version0Length = Math.Min(EmojiColorTable.Version0Length(colrHeader), checked((int)colr.Length));
            return EmojiColorTable.Parse(Read(file, colr.Offset, version0Length), Read(file, cpal.Offset, checked((int)cpal.Length)));
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or FormatException or OverflowException or ArgumentOutOfRangeException)
        {
            PlatformLog.Problem("Reading the emoji font's colors failed", error);
            return null;
        }
    }

    private static Dictionary<string, (uint, uint)> ReadTableDirectory(FileStream file)
    {
        byte[] header = Read(file, 0, 12);
        ushort count = (ushort)((header[4] << 8) | header[5]);
        byte[] records = Read(file, 12, count * 16);
        Dictionary<string, (uint, uint)> tables = [];
        for (int i = 0; i < count; i++)
        {
            int at = i * 16;
            string tag = System.Text.Encoding.ASCII.GetString(records, at, 4);
            tables[tag] = (BigEndian(records, at + 8), BigEndian(records, at + 12));
        }

        return tables;
    }

    private static uint BigEndian(byte[] bytes, int at) =>
        ((uint)bytes[at] << 24) | ((uint)bytes[at + 1] << 16) | ((uint)bytes[at + 2] << 8) | bytes[at + 3];

    private static byte[] Read(FileStream file, long offset, int length)
    {
        byte[] bytes = new byte[length];
        file.Position = offset;
        file.ReadExactly(bytes);
        return bytes;
    }
}
