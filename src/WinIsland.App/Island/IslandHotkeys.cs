// IslandHotkeys.cs: WinIsland's three hotkeys (PRD FR-5), set at start and changed from Settings.
// Applying a set registers all three and returns a short conflict message when another app
// already owns one (shown as a banner at start and in Settings). Release frees them while Settings
// records a new combination.

using WinIsland.Core.Hotkeys;
using WinIsland.Platform.Hotkeys;

namespace WinIsland.App.Island;

internal sealed class IslandHotkeys : IDisposable
{
    public IslandHotkeys(IntPtr windowHandle)
    {
        Global = new GlobalHotkeys(windowHandle);
    }

    /// <summary>The Windows registrations, also used for the music keys while expanded.</summary>
    public GlobalHotkeys Global { get; }

    /// <summary>The set most recently applied, used again after Release.</summary>
    public HotkeySet Current { get; private set; } = HotkeySet.Defaults;

    /// <summary>Registers the set. Returns the conflict text, or null when all three are WinIsland's.</summary>
    public string? Apply(HotkeySet hotkeys)
    {
        ArgumentNullException.ThrowIfNull(hotkeys);
        Current = hotkeys;

        Dictionary<HotkeyAction, Hotkey> wanted = new()
        {
            [HotkeyAction.Expand] = hotkeys.Expand,
            [HotkeyAction.HideShow] = hotkeys.HideShow,
            [HotkeyAction.HideBanner] = hotkeys.HideBanner,
        };

        IReadOnlyList<HotkeyRegistration> results = Global.Register(wanted);
        List<Hotkey> unavailable = results.Where(result => !result.IsRegistered).Select(result => result.Hotkey).ToList();
        return HotkeyConflictMessage.Describe(unavailable);
    }

    /// <summary>Frees the three hotkeys until the next Apply.</summary>
    public void Release()
    {
        Global.Release();
    }

    public void Dispose()
    {
        Global.Dispose();
    }
}
