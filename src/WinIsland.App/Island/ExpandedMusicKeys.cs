// ExpandedMusicKeys.cs: turns the music keys on while the island is expanded and off when it
// closes (Diva's request, 31 Oct 2026): Space plays or pauses, Ctrl+Right next, Ctrl+Left previous.
// While the island is open these keys go to WinIsland instead of the game; once it closes they
// belong to the game again.

using System.Diagnostics;

using WinIsland.Core.Hotkeys;
using WinIsland.Core.Island;
using WinIsland.Platform.Hotkeys;

namespace WinIsland.App.Island;

internal sealed class ExpandedMusicKeys : IDisposable
{
    private readonly IslandStateMachine _island;
    private readonly GlobalHotkeys _hotkeys;
    private bool _isActive;

    public ExpandedMusicKeys(IslandStateMachine island, GlobalHotkeys hotkeys)
    {
        ArgumentNullException.ThrowIfNull(island);
        ArgumentNullException.ThrowIfNull(hotkeys);
        _island = island;
        _hotkeys = hotkeys;
        _island.Changed += (_, _) => FollowIsland();
    }

    public void Dispose()
    {
        _hotkeys.UnregisterMusicShortcuts();
    }

    private void FollowIsland()
    {
        bool shouldBeActive = _island.View == IslandView.Expanded;
        if (shouldBeActive == _isActive)
        {
            return;
        }

        _isActive = shouldBeActive;
        if (!shouldBeActive)
        {
            _hotkeys.UnregisterMusicShortcuts();
            return;
        }

        IReadOnlyList<MusicShortcut> unavailable = _hotkeys.RegisterMusicShortcuts(MusicShortcut.All);
        if (unavailable.Count > 0)
        {
            // Not worth a banner: the buttons on the island still work. Record it for diagnosis.
            Trace.TraceWarning("Music keys owned by another app: {0}", string.Join(", ", unavailable));
        }
    }
}
