// NotificationTracker.cs: remembers which notifications are currently in Windows (memory only)
// and reports each one that appears, changes, or disappears.
// Both the event watcher and the poller feed it, so a notification is reported once even when
// both paths see it. The "seen via" label tells us which path saw it first.

namespace NotificationProbe;

internal sealed class NotificationTracker(ConsoleReport report)
{
    private readonly Dictionary<uint, NotificationSnapshot> _current = [];
    private readonly Lock _gate = new();

    public void Seen(NotificationSnapshot snapshot, string seenVia)
    {
        lock (_gate)
        {
            if (!_current.TryGetValue(snapshot.Id, out NotificationSnapshot? previous))
            {
                _current[snapshot.Id] = snapshot;
                report.Added(snapshot, seenVia);
                return;
            }

            if (!previous.Texts.SequenceEqual(snapshot.Texts))
            {
                _current[snapshot.Id] = snapshot;
                report.Updated(snapshot, seenVia);
            }
        }
    }

    public void Removed(uint id, string seenVia)
    {
        lock (_gate)
        {
            if (_current.Remove(id, out NotificationSnapshot? removed))
            {
                report.Removed(removed, seenVia);
            }
        }
    }

    public IReadOnlyCollection<uint> CurrentIds()
    {
        lock (_gate)
        {
            return _current.Keys.ToList();
        }
    }
}
