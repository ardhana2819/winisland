// SessionReport.cs: prints what a media session reports: track info, which controls the app
// says it supports, shuffle and repeat state, and the timeline (position and seek range).
// The "enabled" flags are the key evidence for U6 (seek) and B3 (shuffle and repeat).

using Windows.Media.Control;

namespace MediaProbe;

internal static class SessionReport
{
    public static void PrintAllSessions(GlobalSystemMediaTransportControlsSessionManager manager)
    {
        IReadOnlyList<GlobalSystemMediaTransportControlsSession> sessions = manager.GetSessions();
        Console.WriteLine($"Media sessions found: {sessions.Count}");
        foreach (GlobalSystemMediaTransportControlsSession session in sessions)
        {
            Console.WriteLine($"  - {session.SourceAppUserModelId}");
        }

        Console.WriteLine($"Current session: {manager.GetCurrentSession()?.SourceAppUserModelId ?? "(none)"}");
    }

    public static async Task PrintStateAsync(GlobalSystemMediaTransportControlsSession session)
    {
        Console.WriteLine();
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Session {session.SourceAppUserModelId}");

        try
        {
            GlobalSystemMediaTransportControlsSessionMediaProperties media = await session.TryGetMediaPropertiesAsync();
            Console.WriteLine($"  Track:      \"{media.Title}\" by \"{media.Artist}\" (album \"{media.AlbumTitle}\")");
            Console.WriteLine($"  Artwork:    {(media.Thumbnail is null ? "none" : "available")}");
        }
        catch (Exception error)
        {
            Console.WriteLine($"  Track:      ERROR {error.GetType().Name} 0x{error.HResult:X8}");
        }

        GlobalSystemMediaTransportControlsSessionPlaybackInfo playback = session.GetPlaybackInfo();
        GlobalSystemMediaTransportControlsSessionPlaybackControls controls = playback.Controls;
        Console.WriteLine($"  Status:     {playback.PlaybackStatus}");
        Console.WriteLine($"  Enabled:    play={controls.IsPlayEnabled} pause={controls.IsPauseEnabled} " +
                          $"playPause={controls.IsPlayPauseToggleEnabled} next={controls.IsNextEnabled} " +
                          $"previous={controls.IsPreviousEnabled}");
        Console.WriteLine($"              seek(playbackPosition)={controls.IsPlaybackPositionEnabled} " +
                          $"shuffle={controls.IsShuffleEnabled} repeat={controls.IsRepeatEnabled}");
        Console.WriteLine($"  Shuffle on: {playback.IsShuffleActive?.ToString() ?? "(not reported)"}");
        Console.WriteLine($"  Repeat:     {playback.AutoRepeatMode?.ToString() ?? "(not reported)"}");

        GlobalSystemMediaTransportControlsSessionTimelineProperties timeline = session.GetTimelineProperties();
        Console.WriteLine($"  Timeline:   position={timeline.Position:mm\\:ss} of {timeline.EndTime:mm\\:ss} " +
                          $"(start {timeline.StartTime:mm\\:ss}), seek range {timeline.MinSeekTime:mm\\:ss} to " +
                          $"{timeline.MaxSeekTime:mm\\:ss}, last updated {timeline.LastUpdatedTime.ToLocalTime():HH:mm:ss}");
    }
}
