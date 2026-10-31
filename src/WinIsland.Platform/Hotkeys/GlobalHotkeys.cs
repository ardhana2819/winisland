// GlobalHotkeys.cs: registers WinIsland's hotkeys with Windows and reports when one is pressed.
// RegisterHotKey is the standard Windows way to get a key combination while another program
// (the game) has focus. It installs no hook into other programs: Windows just sends our window a
// message (PRD section 7). Spike U5 showed this works in Diva's games.

using System.Runtime.InteropServices;

using WinIsland.Core.Hotkeys;
using WinIsland.Platform.Windows;

namespace WinIsland.Platform.Hotkeys;

public sealed class GlobalHotkeys : IDisposable
{
    private readonly IntPtr _windowHandle;
    private readonly List<HotkeyRegistration> _registrations = [];
    private readonly HashSet<HotkeyAction> _musicShortcuts = [];

    public GlobalHotkeys(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
        {
            throw new ArgumentException("Hotkeys need a window handle to receive their messages.", nameof(windowHandle));
        }

        _windowHandle = windowHandle;
    }

    public event EventHandler<HotkeyAction>? Pressed;

    /// <summary>Asks Windows for each hotkey. Every result is returned, including the ones that failed.</summary>
    public IReadOnlyList<HotkeyRegistration> Register(IReadOnlyDictionary<HotkeyAction, Hotkey> hotkeys)
    {
        ArgumentNullException.ThrowIfNull(hotkeys);

        foreach ((HotkeyAction action, Hotkey hotkey) in hotkeys)
        {
            // The action's number doubles as the Windows hotkey id, so a message maps straight back to it.
            int id = (int)action;
            bool registered = NativeMethods.RegisterHotKey(
                _windowHandle, id, ToWindowsModifiers(hotkey.Modifiers) | NativeMethods.ModNoRepeat, ToVirtualKey(hotkey.Key));
            int errorCode = registered ? 0 : Marshal.GetLastPInvokeError();
            _registrations.Add(new HotkeyRegistration(action, hotkey, registered, errorCode));
        }

        return _registrations;
    }

    /// <summary>
    /// Takes the music keys (Space, Ctrl+Right, Ctrl+Left) while the island is expanded.
    /// Returns the shortcuts another app already owns; those simply do not work this time.
    /// </summary>
    public IReadOnlyList<MusicShortcut> RegisterMusicShortcuts(IReadOnlyList<MusicShortcut> shortcuts)
    {
        ArgumentNullException.ThrowIfNull(shortcuts);

        List<MusicShortcut> unavailable = [];
        foreach (MusicShortcut shortcut in shortcuts.Where(item => !_musicShortcuts.Contains(item.Action)))
        {
            bool registered = NativeMethods.RegisterHotKey(
                _windowHandle, (int)shortcut.Action, ToWindowsModifiers(shortcut.Modifiers) | NativeMethods.ModNoRepeat, ToVirtualKey(shortcut.Key));
            if (registered)
            {
                _musicShortcuts.Add(shortcut.Action);
            }
            else
            {
                unavailable.Add(shortcut);
            }
        }

        return unavailable;
    }

    /// <summary>Gives the music keys back to the game and other programs when the island closes.</summary>
    public void UnregisterMusicShortcuts()
    {
        foreach (HotkeyAction action in _musicShortcuts)
        {
            NativeMethods.UnregisterHotKey(_windowHandle, (int)action);
        }

        _musicShortcuts.Clear();
    }

    /// <summary>Call from the window's message hook. Returns true when the message was one of our hotkeys.</summary>
    public bool TryHandleMessage(int message, IntPtr wParam)
    {
        if (message != NativeMethods.WmHotkey)
        {
            return false;
        }

        int id = wParam.ToInt32();
        HotkeyRegistration? match = _registrations.FirstOrDefault(item => item.IsRegistered && (int)item.Action == id);
        if (match is not null)
        {
            Pressed?.Invoke(this, match.Action);
            return true;
        }

        HotkeyAction action = (HotkeyAction)id;
        if (_musicShortcuts.Contains(action))
        {
            Pressed?.Invoke(this, action);
            return true;
        }

        return false;
    }

    public void Dispose()
    {
        foreach (HotkeyRegistration registration in _registrations.Where(item => item.IsRegistered))
        {
            NativeMethods.UnregisterHotKey(_windowHandle, (int)registration.Action);
        }

        _registrations.Clear();
        UnregisterMusicShortcuts();
    }

    private static uint ToWindowsModifiers(HotkeyModifiers modifiers)
    {
        uint result = 0;
        if (modifiers.HasFlag(HotkeyModifiers.Ctrl))
        {
            result |= NativeMethods.ModControl;
        }

        if (modifiers.HasFlag(HotkeyModifiers.Alt))
        {
            result |= NativeMethods.ModAlt;
        }

        if (modifiers.HasFlag(HotkeyModifiers.Shift))
        {
            result |= NativeMethods.ModShift;
        }

        if (modifiers.HasFlag(HotkeyModifiers.Win))
        {
            result |= NativeMethods.ModWin;
        }

        return result;
    }

    private static uint ToVirtualKey(MusicKey key)
    {
        return key switch
        {
            MusicKey.Space => NativeMethods.VkSpace,
            MusicKey.LeftArrow => NativeMethods.VkLeft,
            MusicKey.RightArrow => NativeMethods.VkRight,
            _ => throw new ArgumentOutOfRangeException(nameof(key), key, "Unknown music key."),
        };
    }

    // For letters and digits, the Windows virtual key code is the upper-case ASCII code.
    private static uint ToVirtualKey(char key)
    {
        if (!char.IsAsciiLetterOrDigit(key))
        {
            throw new ArgumentException($"'{key}' is not a letter or digit, so it cannot be a hotkey.", nameof(key));
        }

        return char.ToUpperInvariant(key);
    }
}
