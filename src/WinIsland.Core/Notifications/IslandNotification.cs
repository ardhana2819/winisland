// IslandNotification.cs: a notification as the island shows it: which app, who it is from, and
// the message preview. Held in memory only, never written to disk or logs (PRD FR-18).
// The conversation (app plus sender or chat name) groups messages from the same chat, which the
// "read" and "replaced" rules need (see NotificationFeed.cs).

namespace WinIsland.Core.Notifications;

public sealed record IslandNotification(
    uint WindowsId,
    NotificationSource Source,
    string AppName,
    string Sender,
    string Preview,
    DateTimeOffset ReceivedAt)
{
    public bool IsSameConversation(IslandNotification other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return Source == other.Source && string.Equals(Sender, other.Sender, StringComparison.Ordinal);
    }
}
