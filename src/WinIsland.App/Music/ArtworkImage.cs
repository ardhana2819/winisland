// ArtworkImage.cs: turns album art bytes into a small image for the island.
// The largest artwork on the island is 64 DIPs (expanded view), at most about 100 pixels at the
// biggest scale, so the picture is decoded at 128 pixels wide instead of its full size. That keeps
// memory low (PRD G4) without visible loss.

using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WinIsland.App.Music;

internal static class ArtworkImage
{
    private const int DecodeWidthPixels = 128;

    /// <summary>Returns the decoded image, or null when there are no bytes or they are not a picture.</summary>
    public static ImageSource? FromBytes(byte[]? bytes)
    {
        if (bytes is null || bytes.Length == 0)
        {
            return null;
        }

        try
        {
            using MemoryStream stream = new(bytes);
            BitmapImage image = new();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.DecodePixelWidth = DecodeWidthPixels;
            image.StreamSource = stream;
            image.EndInit();
            image.Freeze();
            return image;
        }
        catch (Exception error) when (error is NotSupportedException or FileFormatException or IOException)
        {
            System.Diagnostics.Trace.TraceWarning("Album art could not be decoded: {0}", error.GetType().Name);
            return null;
        }
    }
}
