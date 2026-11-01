// WhatsAppWebPriority.cs: makes WhatsApp Web come first when WhatsApp desktop also sends
// notifications (PRD FR-29, Diva 1 Nov 2026: "prioritaskan notif WA Web"; spike W4, W5, W7).
// WinIsland does not check whether WhatsApp desktop is running (that would be process inspection,
// PRD section 7); it decides from the notifications alone:
//   - Desktop's one-line "You may have new messages", sent while it is closed, carries no message:
//     always ignored.
//   - A desktop notification for a chat that WhatsApp Web sent one for in the last 10 minutes is
//     ignored: Web covers that chat. This also drops the longer text desktop sends again when it
//     is opened.
// Desktop notifications for chats Web has not covered show as before. The chats Web covered are
// kept in memory only, by chat name, and forgotten after 10 minutes.

namespace WinIsland.Core.Notifications;

public sealed class WhatsAppWebPriority
{
    /// <summary>How long a chat counts as covered by WhatsApp Web after its last Web notification.</summary>
    public static readonly TimeSpan CoverWindow = TimeSpan.FromMinutes(10);

    /// <summary>What WhatsApp desktop shows while it is closed (spike W4, Diva's screenshot).</summary>
    public const string DesktopClosedSummary = "You may have new messages";

    private readonly Dictionary<string, DateTimeOffset> _webChats = new(StringComparer.Ordinal);

    /// <summary>True when this notification should not reach the island at all.</summary>
    public bool ShouldIgnore(IslandNotification notification, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(notification);
        if (notification.Source != NotificationSource.WhatsApp)
        {
            return false;
        }

        Forget(now);
        if (notification.FromBrowser)
        {
            _webChats[notification.Sender] = now;
            return false;
        }

        bool isClosedSummary = string.IsNullOrEmpty(notification.Preview)
            && string.Equals(notification.Sender, DesktopClosedSummary, StringComparison.OrdinalIgnoreCase);
        return isClosedSummary || _webChats.ContainsKey(notification.Sender);
    }

    private void Forget(DateTimeOffset now)
    {
        foreach (string chat in _webChats.Where(pair => now - pair.Value > CoverWindow).Select(pair => pair.Key).ToList())
        {
            _webChats.Remove(chat);
        }
    }
}
