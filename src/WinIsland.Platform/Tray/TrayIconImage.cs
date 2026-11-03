// TrayIconImage.cs: WinIsland's tray icon. Since v1.5 (PRD FR-43) it is the app icon itself,
// WinIsland.ico (concept B, "Mono pill", FR-34), at the tray's small size; before, a white pill was
// drawn in code. While paused the same icon is drawn at about 40 percent opacity, so the paused
// state is visible at a glance (PRD FR-6).

using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

using WinIsland.Platform.Windows;

namespace WinIsland.Platform.Tray;

internal static class TrayIconImage
{
    // About 40 percent opacity: clearly different from the normal icon, still visible.
    private const float DimmedOpacity = 0.4f;

    /// <summary>The icon file at the tray's size, as is or dimmed.</summary>
    public static Icon Create(Stream iconFile, bool dimmed)
    {
        ArgumentNullException.ThrowIfNull(iconFile);
        iconFile.Position = 0;
        Icon icon = new(iconFile, SystemInformation.SmallIconSize);
        if (!dimmed)
        {
            return icon;
        }

        using (icon)
        {
            return Dimmed(icon);
        }
    }

    private static Icon Dimmed(Icon icon)
    {
        using Bitmap source = icon.ToBitmap();
        using Bitmap faded = new(source.Width, source.Height);
        using (Graphics graphics = Graphics.FromImage(faded))
        using (ImageAttributes opacity = new())
        {
            ColorMatrix matrix = new() { Matrix33 = DimmedOpacity };
            opacity.SetColorMatrix(matrix);
            graphics.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height), 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, opacity);
        }

        IntPtr iconHandle = faded.GetHicon();
        try
        {
            // Icon.FromHandle does not own the handle, so take a copy that does, then free the original.
            using Icon borrowed = Icon.FromHandle(iconHandle);
            return (Icon)borrowed.Clone();
        }
        finally
        {
            NativeMethods.DestroyIcon(iconHandle);
        }
    }
}
