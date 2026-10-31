// HotkeyRegistration.cs: the outcome of asking Windows for one global hotkey.

using WinIsland.Core.Hotkeys;
using WinIsland.Platform.Windows;

namespace WinIsland.Platform.Hotkeys;

public sealed record HotkeyRegistration(HotkeyAction Action, Hotkey Hotkey, bool IsRegistered, int ErrorCode)
{
    /// <summary>True when the key combination is already owned by another program.</summary>
    public bool IsTakenByAnotherApp => ErrorCode == NativeMethods.ErrorHotkeyAlreadyRegistered;
}
