// SettingsController.cs: the single place where settings are loaded, checked, applied and saved
// (PRD FR-19, FR-20). A change from the Settings window is checked against the rules in Core
// first; only a valid change is applied at once (island, hotkeys, start with Windows) and saved.
// It also opens the Settings window, from the tray or the conflict banner's Change button.
// Start with Windows is answered later by Windows (LaunchAtStartup), so its problem text comes separately.

using WinIsland.App.Island;
using WinIsland.Core.Island;
using WinIsland.Core.Settings;
using WinIsland.Platform.Settings;

namespace WinIsland.App.Settings;

internal sealed class SettingsController
{
    private readonly IslandStateMachine _island;
    private readonly SemaphoreSlim _startupGate = new(1, 1);
    private IslandHotkeys? _hotkeys;
    private SettingsWindow? _window;

    public SettingsController(IslandStateMachine island, AppSettings saved)
    {
        ArgumentNullException.ThrowIfNull(island);
        ArgumentNullException.ThrowIfNull(saved);
        _island = island;
        Current = saved;
    }

    /// <summary>Raised after a change was applied, so an open Settings window can refresh.</summary>
    public event EventHandler? Changed;

    /// <summary>Raised when Windows answered a start with Windows change, so Settings can show StartupProblemText.</summary>
    public event EventHandler? StartupProblemChanged;

    public AppSettings Current { get; private set; }

    /// <summary>"Alt+X is used by another app", or null when all hotkeys are WinIsland's.</summary>
    public string? HotkeyConflictText { get; private set; }

    /// <summary>Why Windows does not follow the "Launch with Windows" switch, or null when it does.</summary>
    public string? StartupProblemText { get; private set; }

    /// <summary>Reads the saved settings; a missing or damaged file gives the defaults.</summary>
    public static AppSettings LoadSaved() => AppSettingsJson.Read(SettingsFile.Load());

    /// <summary>Applies the settings that do not need the island window: notifications and start with Windows.</summary>
    public void ApplyAtStart()
    {
        _island.UpdateNotificationSettings(Current.Notifications);
        ApplyStartup();
    }

    /// <summary>Called once the island window has registered its hotkeys.</summary>
    public void AttachHotkeys(IslandHotkeys hotkeys, string? startConflict)
    {
        _hotkeys = hotkeys ?? throw new ArgumentNullException(nameof(hotkeys));
        HotkeyConflictText = startConflict;
    }

    /// <summary>Checks and applies a change. Returns the problems; empty means applied and saved.</summary>
    public IReadOnlyList<string> TryChange(AppSettings next)
    {
        ArgumentNullException.ThrowIfNull(next);

        IReadOnlyList<string> problems = next.FindProblems();
        if (problems.Count > 0)
        {
            return problems;
        }

        bool hotkeysChanged = next.Hotkeys != Current.Hotkeys;
        Current = next;
        _island.UpdateNotificationSettings(next.Notifications);
        if (hotkeysChanged && _hotkeys is not null)
        {
            HotkeyConflictText = _hotkeys.Apply(next.Hotkeys);
        }

        ApplyStartup();
        bool saved = SettingsFile.Save(AppSettingsJson.Write(next));
        Changed?.Invoke(this, EventArgs.Empty);
        return saved ? [] : ["The settings work now but could not be saved; they will reset when WinIsland restarts."];
    }

    /// <summary>Frees the hotkeys while Settings records a new key combination.</summary>
    public void PauseHotkeys() => _hotkeys?.Release();

    /// <summary>Takes the hotkeys back after recording was cancelled or refused.</summary>
    public void ResumeHotkeys()
    {
        if (_hotkeys is not null)
        {
            HotkeyConflictText = _hotkeys.Apply(Current.Hotkeys);
        }
    }

    public void OpenWindow()
    {
        if (_window is not null)
        {
            _window.Activate();
            return;
        }

        _window = new SettingsWindow(this);
        _window.Closed += (_, _) => _window = null;
        _window.Show();
        _window.Activate();
    }

    // Fire and forget: the startup task answers asynchronously. A problem shows in an open Settings
    // window through Changed. One change at a time, each reading Current when it runs, so quick
    // on-off clicks always end in the switch's last position.
    private async void ApplyStartup()
    {
        await _startupGate.WaitAsync();
        try
        {
            StartupProblemText = await LaunchAtStartup.ApplyAsync(Current.LaunchAtStartup);
        }
        finally
        {
            _startupGate.Release();
        }

        StartupProblemChanged?.Invoke(this, EventArgs.Empty);
    }
}
