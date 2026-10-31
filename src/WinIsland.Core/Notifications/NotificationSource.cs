// NotificationSource.cs: which app a notification came from, as far as the island cares.
// PRD section 4: WhatsApp desktop, Discord desktop, and a general path for other Windows toasts.

namespace WinIsland.Core.Notifications;

public enum NotificationSource
{
    WhatsApp,
    Discord,
    Other,
}
