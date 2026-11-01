// NotificationFeed.cs: the notifications the island knows about, and their timing rules.
//   FR-15  the last 3 notifications from all apps, newest first, memory only; items leave when
//          Windows removes them (read in the app, spike U3).
//   FR-15  waiting banners, newest first; swipe hides the current one, hotkey 3 hides all.
//   FR-16  one timer for all waiting banners: each new one restarts it, when it ends they all hide.
//   Replacement rule (Diva, 31 Oct 2026): WhatsApp keeps one Windows notification per chat and
//   swaps it for each new message (spike U7). So a removal followed within 1 second by a new
//   notification from the same conversation is a replacement: the older message stays. A removal
//   with nothing after it means the chat was read: every item from that conversation leaves.
//   Repost rule (Diva's bug, 1 Nov 2026): the same text from the same conversation within a minute
//   is one message that WhatsApp posted twice, so it stays one item. The same holds when WhatsApp
//   desktop and WhatsApp Web both send it (v1.2): the later one replaces the earlier, and desktop
//   notifications that WhatsApp Web already covers never arrive (WhatsAppWebPriority.cs), so the
//   item kept is the Web one.
// Time is passed in; NextTimeCheckAt says when the next time-based change is due.

namespace WinIsland.Core.Notifications;

public sealed class NotificationFeed
{
    public const int RecentLimit = 3;
    public static readonly TimeSpan ReplacementWindow = TimeSpan.FromSeconds(1);

    /// <summary>The same text from the same chat within this time is one message posted twice.</summary>
    public static readonly TimeSpan RepostWindow = TimeSpan.FromMinutes(1);

    private readonly List<IslandNotification> _recent = [];
    private readonly List<IslandNotification> _waiting = [];
    private readonly List<PendingRead> _pendingReads = [];
    private readonly WhatsAppWebPriority _whatsAppWeb = new();

    /// <summary>The last 3 notifications, newest first (FR-15).</summary>
    public IReadOnlyList<IslandNotification> Recent => _recent;

    /// <summary>Notifications waiting to be seen as banners, newest first.</summary>
    public IReadOnlyList<IslandNotification> Waiting => _waiting;

    public IslandNotification? CurrentBanner => _waiting.Count > 0 ? _waiting[0] : null;

    public DateTimeOffset? BannerEndsAt { get; private set; }

    public DateTimeOffset? NextTimeCheckAt
    {
        get
        {
            DateTimeOffset? earliest = BannerEndsAt;
            foreach (PendingRead pending in _pendingReads)
            {
                if (earliest is null || pending.DecideAt < earliest)
                {
                    earliest = pending.DecideAt;
                }
            }

            return earliest;
        }
    }

    /// <summary>A new notification. Returns false when it was already known (seen twice by event and polling).</summary>
    public bool Add(IslandNotification notification, DateTimeOffset now, TimeSpan bannerDuration)
    {
        ArgumentNullException.ThrowIfNull(notification);
        if (_recent.Any(item => item.WindowsId == notification.WindowsId))
        {
            return false;
        }

        // Checked before anything else: an ignored desktop notification must not count as WhatsApp
        // swapping its notification, so a chat read in desktop still leaves (spike W7).
        if (_whatsAppWeb.ShouldIgnore(notification, now))
        {
            return false;
        }

        // A removal just before, from the same conversation, was WhatsApp swapping its notification.
        _pendingReads.RemoveAll(pending => pending.Conversation.IsSameConversation(notification));

        // WhatsApp sometimes posts the same message again under a new Windows id (Diva's bug,
        // 1 Nov 2026: one message showed as two cards). Same chat, same text, within a minute:
        // keep one item, under the new id so a later removal still finds it, and show no new banner.
        int repost = _recent.FindIndex(item => IsRepost(item, notification));
        if (repost >= 0)
        {
            uint oldId = _recent[repost].WindowsId;
            _recent[repost] = notification;
            int waiting = _waiting.FindIndex(item => item.WindowsId == oldId);
            if (waiting >= 0)
            {
                _waiting[waiting] = notification;
            }

            return false;
        }

        _recent.Insert(0, notification);
        if (_recent.Count > RecentLimit)
        {
            _recent.RemoveRange(RecentLimit, _recent.Count - RecentLimit);
        }

        _waiting.Insert(0, notification);
        BannerEndsAt = now + bannerDuration;
        return true;
    }

    /// <summary>Windows removed a notification. Decided 1 second later: replaced or read.</summary>
    public void MarkRemoved(uint windowsId, DateTimeOffset now)
    {
        IslandNotification? known = _recent.FirstOrDefault(item => item.WindowsId == windowsId)
            ?? _waiting.FirstOrDefault(item => item.WindowsId == windowsId);
        if (known is null || _pendingReads.Any(pending => pending.Conversation.IsSameConversation(known)))
        {
            return;
        }

        // A newer message from the same chat is already here: WhatsApp swapped its notification.
        // Polling sees the new one and the removal in the same check and reports the new one first
        // (Diva's fast-message bug, 31 Oct 2026). Reading a chat removes its newest notification,
        // which has nothing newer, so the read rule below still applies then.
        if (HasNewerFromSameConversation(_recent, known) || HasNewerFromSameConversation(_waiting, known))
        {
            return;
        }

        _pendingReads.Add(new PendingRead(known, now + ReplacementWindow));
    }

    /// <summary>Applies everything that is due. Returns true when something changed.</summary>
    public bool UpdateTime(DateTimeOffset now)
    {
        bool changed = false;

        foreach (PendingRead read in _pendingReads.Where(pending => now >= pending.DecideAt).ToList())
        {
            _pendingReads.Remove(read);
            changed |= _recent.RemoveAll(item => item.IsSameConversation(read.Conversation)) > 0;
            changed |= _waiting.RemoveAll(item => item.IsSameConversation(read.Conversation)) > 0;
        }

        if (_waiting.Count == 0)
        {
            BannerEndsAt = null;
        }

        if (BannerEndsAt is DateTimeOffset end && now >= end)
        {
            HideAllBanners();
            changed = true;
        }

        return changed;
    }

    /// <summary>Swipe: hides the current banner and shows the next one, with a fresh timer.</summary>
    public void HideCurrentBanner(DateTimeOffset now, TimeSpan bannerDuration)
    {
        if (_waiting.Count == 0)
        {
            return;
        }

        _waiting.RemoveAt(0);
        BannerEndsAt = _waiting.Count > 0 ? now + bannerDuration : null;
    }

    /// <summary>Hotkey 3, the banner timer, or opening the list: no banners waiting any more.</summary>
    public void HideAllBanners()
    {
        _waiting.Clear();
        BannerEndsAt = null;
    }

    private static bool IsRepost(IslandNotification known, IslandNotification incoming)
    {
        return known.IsSameConversation(incoming)
            && string.Equals(known.Preview, incoming.Preview, StringComparison.Ordinal)
            && (incoming.ReceivedAt - known.ReceivedAt).Duration() <= RepostWindow;
    }

    // Lists are newest first, so anything before the item is newer.
    private static bool HasNewerFromSameConversation(List<IslandNotification> newestFirst, IslandNotification removed)
    {
        int index = newestFirst.FindIndex(item => item.WindowsId == removed.WindowsId);
        return index > 0 && newestFirst.Take(index).Any(item => item.IsSameConversation(removed));
    }

    private sealed record PendingRead(IslandNotification Conversation, DateTimeOffset DecideAt);
}
