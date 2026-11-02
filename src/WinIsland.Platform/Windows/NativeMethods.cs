// NativeMethods.cs: the Win32 functions Platform needs, declared in one place.
// They only act on WinIsland's own windows and icons, or read the screen size.
// None of them touch another process (PRD section 7, safety with games).

using System.Runtime.InteropServices;

namespace WinIsland.Platform.Windows;

internal static partial class NativeMethods
{
    public const int GwlExStyle = -20;
    public const long WsExTopmost = 0x00000008;
    public const long WsExToolWindow = 0x00000080;
    public const long WsExNoActivate = 0x08000000;

    public const int WmMouseActivate = 0x0021;
    public const int MaNoActivate = 3;

    public static readonly IntPtr HwndTopmost = new(-1);
    public const uint SwpNoSize = 0x0001;
    public const uint SwpNoMove = 0x0002;
    public const uint SwpNoActivate = 0x0010;

    public const int SmCxScreen = 0;

    /// <summary>GW_HWNDPREV: the window just above in the z-order.</summary>
    public const uint GwHwndPrev = 3;

    [LibraryImport("user32.dll")]
    public static partial IntPtr GetWindow(IntPtr windowHandle, uint command);

    public const int WmHotkey = 0x0312;
    public const uint ModAlt = 0x0001;
    public const uint ModControl = 0x0002;
    public const uint ModShift = 0x0004;
    public const uint ModWin = 0x0008;
    public const uint ModNoRepeat = 0x4000;

    /// <summary>ERROR_HOTKEY_ALREADY_REGISTERED: another program already owns this key combination.</summary>
    public const int ErrorHotkeyAlreadyRegistered = 1409;

    public const uint VkSpace = 0x20;
    public const uint VkLeft = 0x25;
    public const uint VkUp = 0x26;
    public const uint VkRight = 0x27;
    public const uint VkDown = 0x28;
    public const int VkLeftButton = 0x01;
    public const int VkRightButton = 0x02;
    public const int VkMiddleButton = 0x04;

    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
        public int X;
        public int Y;
    }

    /// <summary>Reads whether a key or mouse button is down right now. It only reads, it hooks nothing.</summary>
    [LibraryImport("user32.dll")]
    public static partial short GetAsyncKeyState(int virtualKey);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetCursorPos(out Point point);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool RegisterHotKey(IntPtr windowHandle, int id, uint modifiers, uint virtualKey);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool UnregisterHotKey(IntPtr windowHandle, int id);

    [LibraryImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    public static partial IntPtr GetWindowLongPtr(IntPtr windowHandle, int index);

    [LibraryImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    public static partial IntPtr SetWindowLongPtr(IntPtr windowHandle, int index, IntPtr newValue);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetWindowPos(IntPtr windowHandle, IntPtr insertAfter, int x, int y, int width, int height, uint flags);

    [LibraryImport("user32.dll")]
    public static partial int GetSystemMetrics(int index);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DestroyIcon(IntPtr iconHandle);

    // v1.4 Glass look (PRD FR-37): the blur window behind the island.
    public const long WsExTransparent = 0x00000020;
    public const int DwmwaUseHostBackdropBrush = 17;
    public const int DwmwaWindowCornerPreference = 33;
    public const int DwmwcpDoNotRound = 1;
    public const int DqTypeThreadCurrent = 2;
    public const int DqtatComSta = 2;

    [StructLayout(LayoutKind.Sequential)]
    public struct Margins
    {
        public int Left;
        public int Right;
        public int Top;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DispatcherQueueOptions
    {
        public int Size;
        public int ThreadType;
        public int ApartmentType;
    }

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

    [LibraryImport("CoreMessaging.dll")]
    public static partial int CreateDispatcherQueueController(DispatcherQueueOptions options, out IntPtr controller);
}
