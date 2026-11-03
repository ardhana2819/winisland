// MusicSource.cs: decides which Windows media session is the music WinIsland shows (PRD FR-41).
// v1.5: music comes from every app that tells Windows what it plays (Spotify, a browser, Apple
// Music, ...), still only through the Windows media session API. Before v1.5 only Spotify counted.
// The rule (Diva approved, 3 Nov 2026):
//   - the app that is playing;
//   - if several play, the one Windows marks as current, or else the first that plays;
//   - if none plays, the one Windows marks as current (paused), or else the first one.

namespace WinIsland.Core.Music;

/// <summary>One media session Windows lists: its app id and whether it is playing now.</summary>
public sealed record MediaSessionCandidate(string AppId, bool IsPlaying);

public static class MusicSource
{
    /// <summary>The app id of the session to show, or null when Windows lists none.</summary>
    public static string? Choose(IReadOnlyList<MediaSessionCandidate> sessions, string? windowsCurrentAppId)
    {
        ArgumentNullException.ThrowIfNull(sessions);
        if (sessions.Count == 0)
        {
            return null;
        }

        List<MediaSessionCandidate> playing = [.. sessions.Where(session => session.IsPlaying)];
        IReadOnlyList<MediaSessionCandidate> pool = playing.Count > 0 ? playing : sessions;
        MediaSessionCandidate? current = pool.FirstOrDefault(session => string.Equals(session.AppId, windowsCurrentAppId, StringComparison.Ordinal));
        return (current ?? pool[0]).AppId;
    }
}
