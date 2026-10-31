// HotkeyRegistrar.cs: registers the test hotkeys with Windows and counts each press.
// RegisterHotKey is the standard Windows API for global hotkeys. It installs no hook into
// other programs; Windows simply sends our window a message when the combination is pressed.

using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace OverlayProbe;

internal sealed class HotkeyRegistrar
{
    private readonly HwndSource _source;
    private readonly Action<HotkeyDefinition> _onPressed;
    private readonly List<HotkeyResult> _results = [];

    public HotkeyRegistrar(HwndSource source, Action<HotkeyDefinition> onPressed)
    {
        _source = source;
        _onPressed = onPressed;
        _source.AddHook(OnWindowMessage);
    }

    public IReadOnlyList<HotkeyResult> Results => _results;

    public void RegisterAll()
    {
        foreach (HotkeyDefinition hotkey in HotkeyDefinition.WinIslandDefaults)
        {
            bool registered = NativeMethods.RegisterHotKey(
                _source.Handle, hotkey.Id, hotkey.Modifiers | NativeMethods.ModNoRepeat, hotkey.VirtualKey);
            int errorCode = registered ? 0 : Marshal.GetLastPInvokeError();
            _results.Add(new HotkeyResult(hotkey, registered, errorCode));
        }
    }

    public void UnregisterAll()
    {
        foreach (HotkeyResult result in _results)
        {
            NativeMethods.UnregisterHotKey(_source.Handle, result.Hotkey.Id);
        }
    }

    private IntPtr OnWindowMessage(IntPtr windowHandle, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (message != NativeMethods.WmHotkey)
        {
            return IntPtr.Zero;
        }

        HotkeyResult? result = _results.FirstOrDefault(item => item.Hotkey.Id == wParam.ToInt32());
        if (result is not null)
        {
            result.PressCount++;
            handled = true;
            _onPressed(result.Hotkey);
        }

        return IntPtr.Zero;
    }
}
