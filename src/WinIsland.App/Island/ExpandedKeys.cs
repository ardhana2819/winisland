// ExpandedKeys.cs: turns the expanded island's keys on while it is open and off when it closes.
// Music (Diva's request, 31 Oct 2026): Space plays or pauses, Ctrl+Right next, Ctrl+Left previous.
// Pages (PRD FR-21, v1.1): Alt+Down shows the notifications, Alt+Up the music.
// Chats (PRD FR-35, v1.4): Alt+Left goes back to the list, taken only while a chat is open.
// While the island is open these keys go to WinIsland instead of the game; once it closes they
// belong to the game again.

using System.Diagnostics;

using WinIsland.Core.Hotkeys;
using WinIsland.Core.Island;
using WinIsland.Platform.Hotkeys;

namespace WinIsland.App.Island;

internal sealed class ExpandedKeys : IDisposable
{
    private readonly IslandStateMachine _island;
    private readonly GlobalHotkeys _hotkeys;
    private bool _isActive;
    private bool _isBackActive;

    public ExpandedKeys(IslandStateMachine island, GlobalHotkeys hotkeys)
    {
        ArgumentNullException.ThrowIfNull(island);
        ArgumentNullException.ThrowIfNull(hotkeys);
        _island = island;
        _hotkeys = hotkeys;
        _island.Changed += (_, _) => FollowIsland();
    }

    public void Dispose()
    {
        _hotkeys.UnregisterExpandedShortcuts();
    }

    private void FollowIsland()
    {
        FollowPageKeys();
        FollowChat();
    }

    private void FollowChat()
    {
        bool shouldBeActive = _isActive && _island.OpenChat is not null;
        if (shouldBeActive == _isBackActive)
        {
            return;
        }

        _isBackActive = shouldBeActive;
        if (!shouldBeActive)
        {
            _hotkeys.UnregisterExpandedShortcut(HotkeyAction.CloseChat);
            return;
        }

        if (_hotkeys.RegisterExpandedShortcuts([ExpandedShortcut.BackToList]).Count > 0)
        {
            // Clicking the chat still goes back. Record it for diagnosis.
            Trace.TraceWarning("Island key owned by another app: {0}", ExpandedShortcut.BackToList);
        }
    }

    private void FollowPageKeys()
    {
        bool shouldBeActive = _island.View == IslandView.Expanded;
        if (shouldBeActive == _isActive)
        {
            return;
        }

        _isActive = shouldBeActive;
        if (!shouldBeActive)
        {
            _hotkeys.UnregisterExpandedShortcuts();
            return;
        }

        IReadOnlyList<ExpandedShortcut> unavailable = _hotkeys.RegisterExpandedShortcuts(ExpandedShortcut.All);
        if (unavailable.Count > 0)
        {
            // Not worth a banner: the buttons, scrolling and page dots still work. Record it for diagnosis.
            Trace.TraceWarning("Island keys owned by another app: {0}", string.Join(", ", unavailable));
        }
    }
}
