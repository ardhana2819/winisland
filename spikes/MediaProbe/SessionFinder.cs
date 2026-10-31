// SessionFinder.cs: picks the Spotify desktop app's session out of all media sessions.
// Windows lists every app that plays media (browsers, games, Spotify); we only want Spotify.

using Windows.Media.Control;

namespace MediaProbe;

internal static class SessionFinder
{
    public static GlobalSystemMediaTransportControlsSession? FindSpotify(GlobalSystemMediaTransportControlsSessionManager manager)
    {
        return manager.GetSessions().FirstOrDefault(session =>
            session.SourceAppUserModelId.Contains("Spotify", StringComparison.OrdinalIgnoreCase));
    }
}
