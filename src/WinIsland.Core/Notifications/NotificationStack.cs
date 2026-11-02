// NotificationStack.cs: one chat on the notifications page (PRD FR-35, v1.4). Messages from the
// same chat (same app and chat name, the rule in IslandNotification.cs) stack into one card, like
// the iPhone. The card shows the newest message and how many messages the chat has sent; opening
// it shows the last 3. Older message text is not kept: memory only, at most 3 texts per chat
// (FR-18).

namespace WinIsland.Core.Notifications;

public sealed record NotificationStack(IReadOnlyList<IslandNotification> Messages, int Count)
{
    /// <summary>Above this the card says "99+" (FR-35).</summary>
    public const int CountShownLimit = 99;

    /// <summary>The newest message: the one the card shows.</summary>
    public IslandNotification Newest => Messages[0];

    /// <summary>A stack of 2 or more can be opened to show its last messages (FR-35).</summary>
    public bool CanOpen => Count > 1;

    /// <summary>"3 messages", or "99+ messages"; empty for a single message.</summary>
    public string CountText => Count switch
    {
        <= 1 => string.Empty,
        > CountShownLimit => $"{CountShownLimit}+ messages",
        _ => $"{Count} messages",
    };
}
