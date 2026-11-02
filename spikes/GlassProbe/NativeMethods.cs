// NativeMethods.cs: the Win32 and DWM functions GlassProbe needs, declared in one place.
// All of them act on GlassProbe's own window. None of them touch another process or read the screen.

using System.Runtime.InteropServices;

namespace GlassProbe;

internal static partial class NativeMethods
{
    public const int GwlExStyle = -20;
    public const long WsExToolWindow = 0x00000080;
    public const long WsExNoActivate = 0x08000000;
    public const int WmMouseActivate = 0x0021;
    public const int MaNoActivate = 3;
    public const int WmNcActivate = 0x0086;

    // SetWindowCompositionAttribute (undocumented, in user32 since Windows 10): the older way to ask
    // for a blurred backdrop, which does not depend on the window being active.
    public const int WcaAccentPolicy = 19;
    public const int AccentEnableBlurBehind = 3;
    public const int AccentEnableAcrylicBlurBehind = 4;

    // DwmSetWindowAttribute values (dwmapi.h, Windows 11).
    public const int DwmwaUseImmersiveDarkMode = 20;
    public const int DwmwaWindowCornerPreference = 33;
    public const int DwmwaSystemBackdropType = 38;
    public const int DwmwcpDoNotRound = 1;

    [StructLayout(LayoutKind.Sequential)]
    public struct Margins
    {
        public int Left;
        public int Right;
        public int Top;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AccentPolicy
    {
        public int AccentState;
        public int AccentFlags;
        public uint GradientColor; // 0xAABBGGRR
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WindowCompositionAttributeData
    {
        public int Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    [LibraryImport("user32.dll")]
    public static partial int SetWindowCompositionAttribute(IntPtr windowHandle, ref WindowCompositionAttributeData data);

    [LibraryImport("user32.dll", EntryPoint = "DefWindowProcW")]
    public static partial IntPtr DefWindowProc(IntPtr windowHandle, int message, IntPtr wParam, IntPtr lParam);

    [LibraryImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    public static partial IntPtr GetWindowLongPtr(IntPtr windowHandle, int index);

    [LibraryImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    public static partial IntPtr SetWindowLongPtr(IntPtr windowHandle, int index, IntPtr newValue);

    [LibraryImport("dwmapi.dll")]
    public static partial int DwmExtendFrameIntoClientArea(IntPtr windowHandle, ref Margins margins);

    [LibraryImport("dwmapi.dll")]
    public static partial int DwmSetWindowAttribute(IntPtr windowHandle, int attribute, ref int value, int size);

    /// <summary>A rounded rectangle; corner width and height are the full ellipse sizes, in pixels.</summary>
    [LibraryImport("gdi32.dll")]
    public static partial IntPtr CreateRoundRectRgn(int left, int top, int right, int bottom, int cornerWidth, int cornerHeight);

    /// <summary>Gives the window this shape. Windows owns the region afterwards, so it is not freed here.</summary>
    [LibraryImport("user32.dll")]
    public static partial int SetWindowRgn(IntPtr windowHandle, IntPtr region, [MarshalAs(UnmanagedType.Bool)] bool redraw);
}
