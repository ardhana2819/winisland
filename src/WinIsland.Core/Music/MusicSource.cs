// MusicSource.cs: decides which Windows media session is the music WinIsland shows (PRD FR-41).
// v1.5: music comes from every app that tells Windows what it plays (Spotify, a browser, Apple
// Music, ...), still only through the Windows media session API. Before v1.5 only Spotify counted.
// The rule (Diva approved, 3 Nov 2026):
//   - the app that is playing; an app that is changing songs counts as playing;
//   - a music player that plays comes before a video (Diva, 3 Nov 2026): Spotify, Apple Music,
//     YouTube Music installed as an app, or any app that names an album, which YouTube Music in a
//     browser does and a YouTube video does not;
//   - if several play, the one already on the island stays (Diva, 3 Nov 2026: with Spotify and
//     YouTube Music both playing, a skip must reach only the one shown, so the island must not
//     jump to the other while Spotify changes songs); otherwise the one Windows marks as current,
//     or else the first that plays;
//   - an app that was just paused keeps the island for 3 seconds (PauseHold), unless a music
//     player plays while the island shows a video;
//   - if none plays, the one Windows marks as current (paused), or else the first one.

namespace WinIsland.Core.Music;

/// <summary>
/// One media session Windows lists: its app id, whether it is playing (or changing songs) now, and
/// whether what it plays names an album.
/// </summary>
public sealed record MediaSessionCandidate(string AppId, bool IsPlaying, bool HasAlbum = false);

public static class MusicSource
{
    // Parts of the app ids of music players. The last one is YouTube Music installed from Chrome.
    private static readonly string[] MusicPlayerIds = ["Spotify", "AppleMusic", "_crx_cinhimbnkkaeohfgghhklpknlkffjgod"];

    /// <summary>The app id of the session to show, or null when Windows lists none.</summary>
    /// <param name="isShownHeld">True while the app on the island was paused less than 3 seconds ago (PauseHold).</param>
    public static string? Choose(IReadOnlyList<MediaSessionCandidate> sessions, string? windowsCurrentAppId, string? shownAppId = null, bool isShownHeld = false)
    {
        ArgumentNullException.ThrowIfNull(sessions);
        if (sessions.Count == 0)
        {
            return null;
        }

        MediaSessionCandidate? shown = sessions.FirstOrDefault(session => session.AppId == shownAppId);
        List<MediaSessionCandidate> playing = [.. sessions.Where(session => session.IsPlaying)];
        List<MediaSessionCandidate> playingMusic = [.. playing.Where(IsMusicPlayer)];

        bool musicTakesOver = playingMusic.Count > 0 && shown is not null && !IsMusicPlayer(shown);
        if (shown is not null && isShownHeld && !musicTakesOver)
        {
            return shown.AppId;
        }

        IReadOnlyList<MediaSessionCandidate> pool = playingMusic.Count > 0 ? playingMusic : playing.Count > 0 ? playing : sessions;
        if (playing.Count > 0 && shown is not null && pool.Contains(shown))
        {
            return shown.AppId;
        }

        MediaSessionCandidate? current = pool.FirstOrDefault(session => string.Equals(session.AppId, windowsCurrentAppId, StringComparison.Ordinal));
        return (current ?? pool[0]).AppId;
    }

    /// <summary>Whether the session is a music player rather than a video (see the rule above).</summary>
    public static bool IsMusicPlayer(MediaSessionCandidate session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return session.HasAlbum || MusicPlayerIds.Any(id => session.AppId.Contains(id, StringComparison.OrdinalIgnoreCase));
    }
}
