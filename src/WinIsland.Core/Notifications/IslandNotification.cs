// IslandNotification.cs: a notification as the island shows it: which app, who it is from, and
// the message preview. Held in memory only, never written to disk or logs (PRD FR-18).
// The conversation (app plus sender or chat name) groups messages from the same chat, which the
// "read" and "replaced" rules need (see NotificationFeed.cs). WhatsApp desktop and WhatsApp Web
// (FromBrowser, v1.2) share one conversation, so the same chat is one chat from either.

namespace WinIsland.Core.Notifications;

public sealed record IslandNotification(
    uint WindowsId,
    NotificationSource Source,
    string AppName,
    string Sender,
    string Preview,
    DateTimeOffset ReceivedAt,
    bool FromBrowser = false)
{
    public bool IsSameConversation(IslandNotification other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return Source == other.Source && string.Equals(Sender, other.Sender, StringComparison.Ordinal);
    }
}
