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
}
