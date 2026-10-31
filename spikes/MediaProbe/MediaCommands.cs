// MediaCommands.cs: sends one control command to the session and prints Windows' answer.
// Each "Try..." call returns true if the app accepted the request. We print that answer and
// then read the state back, because an app can say "accepted" and still do nothing.

using Windows.Media;
using Windows.Media.Control;

namespace MediaProbe;

internal sealed class MediaCommands(GlobalSystemMediaTransportControlsSession session)
{
    private static readonly TimeSpan SeekStep = TimeSpan.FromSeconds(30);

    public async Task<bool> RunAsync(ConsoleKey key)
    {
        try
        {
            switch (key)
            {
                case ConsoleKey.P:
                    Print("Play/pause", await session.TryTogglePlayPauseAsync());
                    return true;
                case ConsoleKey.N:
                    Print("Next", await session.TrySkipNextAsync());
                    return true;
                case ConsoleKey.B:
                    Print("Previous", await session.TrySkipPreviousAsync());
                    return true;
                case ConsoleKey.S:
                    await SeekByAsync(SeekStep);
                    return true;
                case ConsoleKey.A:
                    await SeekByAsync(-SeekStep);
                    return true;
                case ConsoleKey.F:
                    await ToggleShuffleAsync();
                    return true;
                case ConsoleKey.R:
                    await CycleRepeatAsync();
                    return true;
                case ConsoleKey.I:
                    return true;
                default:
                    return false;
            }
        }
        catch (Exception error)
        {
            Console.WriteLine($">> {key}: ERROR {error.GetType().Name} 0x{error.HResult:X8} {error.Message}");
            return true;
        }
    }

    private async Task SeekByAsync(TimeSpan step)
    {
        TimeSpan current = session.GetTimelineProperties().Position;
        TimeSpan target = current + step < TimeSpan.Zero ? TimeSpan.Zero : current + step;
        bool accepted = await session.TryChangePlaybackPositionAsync(target.Ticks);
        Print($"Seek from {current:mm\\:ss} to {target:mm\\:ss}", accepted);
    }

    private async Task ToggleShuffleAsync()
    {
        bool currentlyOn = session.GetPlaybackInfo().IsShuffleActive ?? false;
        Print($"Shuffle {(currentlyOn ? "off" : "on")}", await session.TryChangeShuffleActiveAsync(!currentlyOn));
    }

    private async Task CycleRepeatAsync()
    {
        MediaPlaybackAutoRepeatMode current = session.GetPlaybackInfo().AutoRepeatMode ?? MediaPlaybackAutoRepeatMode.None;
        MediaPlaybackAutoRepeatMode next = current switch
        {
            MediaPlaybackAutoRepeatMode.None => MediaPlaybackAutoRepeatMode.List,
            MediaPlaybackAutoRepeatMode.List => MediaPlaybackAutoRepeatMode.Track,
            _ => MediaPlaybackAutoRepeatMode.None,
        };
        Print($"Repeat {current} -> {next}", await session.TryChangeAutoRepeatModeAsync(next));
    }

    private static void Print(string command, bool accepted)
    {
        Console.WriteLine($">> {command}: Windows says {(accepted ? "ACCEPTED" : "REJECTED")}");
    }
}
