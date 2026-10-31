// NativeMethods.cs: the few Win32 functions OverlayProbe needs, declared in one place.
// All of them act on our own window or read only which window is in front. None of them
// touch another process.

using System.Runtime.InteropServices;

namespace OverlayProbe;

internal static partial class NativeMethods
{
    public const int GwlExStyle = -20;
    public const long WsExTopmost = 0x00000008;
    public const long WsExToolWindow = 0x00000080;
    public const long WsExNoActivate = 0x08000000;

    public const int WmMouseActivate = 0x0021;
    public const int WmHotkey = 0x0312;
    public const int MaNoActivate = 3;

    public const uint ModAlt = 0x0001;
    public const uint ModControl = 0x0002;
    public const uint ModNoRepeat = 0x4000;

    public static readonly IntPtr HwndTopmost = new(-1);
    public const uint SwpNoSize = 0x0001;
    public const uint SwpNoMove = 0x0002;
    public const uint SwpNoActivate = 0x0010;

    [LibraryImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    public static partial IntPtr GetWindowLongPtr(IntPtr windowHandle, int index);

    [LibraryImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    public static partial IntPtr SetWindowLongPtr(IntPtr windowHandle, int index, IntPtr newValue);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetWindowPos(IntPtr windowHandle, IntPtr insertAfter, int x, int y, int width, int height, uint flags);

    [LibraryImport("user32.dll")]
    public static partial IntPtr GetForegroundWindow();

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool RegisterHotKey(IntPtr windowHandle, int id, uint modifiers, uint virtualKey);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool UnregisterHotKey(IntPtr windowHandle, int id);
}
