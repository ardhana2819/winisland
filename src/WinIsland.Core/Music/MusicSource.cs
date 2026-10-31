// MusicSource.cs: decides which Windows media session is the music WinIsland shows.
// PRD: music comes only from the Spotify desktop app. Windows also lists browsers and other
// players, and its own "current session" can be a browser (spike: it picked Brave), so the
// island picks Spotify by its app id instead. Both the installer version ("Spotify.exe") and the
// Microsoft Store version ("SpotifyAB.SpotifyMusic_...!Spotify") contain "Spotify".

namespace WinIsland.Core.Music;

public static class MusicSource
{
    public static bool IsSpotify(string? appId)
    {
        return !string.IsNullOrWhiteSpace(appId)
            && appId.Contains("Spotify", StringComparison.OrdinalIgnoreCase);
    }
}
