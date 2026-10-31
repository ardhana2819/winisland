// TrayIconImage.cs: draws WinIsland's tray icon at run time: a small white pill on a clear
// background, so it is visible on the dark Windows 11 taskbar.
// Drawing it in code avoids keeping a binary .ico file in the repo.

using System.Drawing.Drawing2D;

using WinIsland.Platform.Windows;

namespace WinIsland.Platform.Tray;

internal static class TrayIconImage
{
    private const int SizePixels = 32;
    private const int PillHeightPixels = 14;
    private const int SidePaddingPixels = 2;

    public static Icon Create()
    {
        using Bitmap bitmap = new(SizePixels, SizePixels);
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(Color.Transparent);
            using GraphicsPath pill = CreatePillPath();
            using SolidBrush brush = new(Color.White);
            graphics.FillPath(brush, pill);
        }

        IntPtr iconHandle = bitmap.GetHicon();
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

    private static GraphicsPath CreatePillPath()
    {
        int width = SizePixels - (2 * SidePaddingPixels);
        int top = (SizePixels - PillHeightPixels) / 2;
        int diameter = PillHeightPixels;

        GraphicsPath path = new();
        path.AddArc(SidePaddingPixels, top, diameter, diameter, 90, 180);
        path.AddArc(SidePaddingPixels + width - diameter, top, diameter, diameter, 270, 180);
        path.CloseFigure();
        return path;
    }
}
