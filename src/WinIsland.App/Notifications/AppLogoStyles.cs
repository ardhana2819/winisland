// AppLogoStyles.cs: reads an app logo's pixels once and asks Core how to draw it (PRD FR-42,
// AppLogoStyle.cs): a white shape on the app's colour, or the full tile. The answer is kept with
// the logo picture, so each app's logo is read only once.

using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using WinIsland.Core.Notifications;

namespace WinIsland.App.Notifications;

internal static class AppLogoStyles
{
    private static readonly ConditionalWeakTable<ImageSource, AppLogoStyle> Known = [];

    /// <summary>The style for a logo, or null when its pixels cannot be read.</summary>
    public static AppLogoStyle? For(ImageSource logo)
    {
        ArgumentNullException.ThrowIfNull(logo);
        if (Known.TryGetValue(logo, out AppLogoStyle? known))
        {
            return known;
        }

        if (logo is not BitmapSource bitmap || bitmap.PixelWidth == 0)
        {
            return null;
        }

        FormatConvertedBitmap pixels = new(bitmap, PixelFormats.Bgra32, null, 0);
        int stride = pixels.PixelWidth * 4;
        byte[] buffer = new byte[stride * pixels.PixelHeight];
        pixels.CopyPixels(buffer, stride, 0);
        AppLogoStyle style = AppLogoStyle.FromBgra32(buffer);
        Known.AddOrUpdate(logo, style);
        return style;
    }
}
