// ArtworkColors.cs: reads the album art's pixels and hands them to Core, which works out the music
// page's colors (PRD FR-25, DESIGN_SPEC section 9). The picture is read at 64 x 64, the size the
// design's steps use, once per song and off the screen thread. In memory only, like the artwork.

using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using WinIsland.Core.Music;

namespace WinIsland.App.Music;

internal static class ArtworkColors
{
    private const int SampleSizePixels = 64;

    /// <summary>The palette for the album art, or Black when there is no picture or it cannot be read.</summary>
    public static ArtworkPalette FromBytes(byte[]? bytes)
    {
        if (bytes is null || bytes.Length == 0)
        {
            return ArtworkPalette.Black;
        }

        try
        {
            using MemoryStream stream = new(bytes);
            BitmapImage image = new();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.DecodePixelWidth = SampleSizePixels;
            image.DecodePixelHeight = SampleSizePixels;
            image.StreamSource = stream;
            image.EndInit();

            FormatConvertedBitmap pixels = new(image, PixelFormats.Bgra32, null, 0);
            int stride = pixels.PixelWidth * 4;
            byte[] buffer = new byte[stride * pixels.PixelHeight];
            pixels.CopyPixels(buffer, stride, 0);
            return ArtworkPalette.FromBgra32(buffer);
        }
        catch (Exception error) when (error is NotSupportedException or FileFormatException or IOException)
        {
            System.Diagnostics.Trace.TraceWarning("Album art colors could not be read: {0}", error.GetType().Name);
            return ArtworkPalette.Black;
        }
    }
}
