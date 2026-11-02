// IncomingCall.cs: a WhatsApp Web call that is ringing right now (PRD FR-38, v1.4).
// Spike C3 and C5 (2 Nov 2026): WhatsApp Web sends a notification whose first text is the
// caller and whose second ends in "Incoming voice call" or "Incoming video call", after an emoji.
// Windows removes it the moment the call is answered, declined or stops ringing (spike C2).
// Only WhatsApp Web, and only WhatsApp's English words: another language shows calls as normal
// banners (a documented limitation).

namespace WinIsland.Core.Notifications;

public sealed record IncomingCall(IslandNotification Notification, bool IsVideo)
{
    private const string VoiceCallText = "Incoming voice call";
    private const string VideoCallText = "Incoming video call";

    public string Caller => Notification.Sender;

    /// <summary>"Incoming voice call" or "Incoming video call", as the banner says it.</summary>
    public string Description => IsVideo ? VideoCallText : VoiceCallText;

    /// <summary>The call this notification announces, or null when it is not a WhatsApp Web call.</summary>
    public static IncomingCall? From(IslandNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);
        if (notification.Source != NotificationSource.WhatsApp || !notification.FromBrowser)
        {
            return null;
        }

        string text = notification.Preview.TrimEnd();
        if (text.EndsWith(VoiceCallText, StringComparison.Ordinal))
        {
            return new IncomingCall(notification, IsVideo: false);
        }

        return text.EndsWith(VideoCallText, StringComparison.Ordinal) ? new IncomingCall(notification, IsVideo: true) : null;
    }
}
