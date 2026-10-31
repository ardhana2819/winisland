// PlaybackStatus.cs: whether the music player is playing, paused or stopped.
// Platform translates the Windows media session status into these three.

namespace WinIsland.Core.Music;

public enum PlaybackStatus
{
    Stopped,
    Paused,
    Playing,
}
