// Program.cs: entry point of the MediaProbe spike.
//
// Usage:
//   MediaProbe.exe --report   Print all media sessions and Spotify's state once, then exit.
//   MediaProbe.exe            Interactive: press a key to send a command to Spotify.
//     P play/pause   N next   B previous   S seek +30 s   A seek -30 s
//     F toggle shuffle   R cycle repeat   I show state again   Q quit

using MediaProbe;

using Windows.Media.Control;

GlobalSystemMediaTransportControlsSessionManager manager =
    await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

SessionReport.PrintAllSessions(manager);

GlobalSystemMediaTransportControlsSession? spotify = SessionFinder.FindSpotify(manager);
if (spotify is null)
{
    Console.WriteLine("No Spotify session found. Start the Spotify desktop app and play a song, then run again.");
    return 1;
}

await SessionReport.PrintStateAsync(spotify);

if (args.Contains("--report"))
{
    return 0;
}

MediaCommands commands = new(spotify);
Console.WriteLine();
Console.WriteLine("Keys: P play/pause, N next, B previous, S seek +30s, A seek -30s, F shuffle, R repeat, I info, Q quit");

while (true)
{
    ConsoleKey key = Console.ReadKey(intercept: true).Key;
    if (key == ConsoleKey.Q)
    {
        return 0;
    }

    bool known = await commands.RunAsync(key);
    if (known)
    {
        // Give Spotify a moment to apply the command before reading the state back.
        await Task.Delay(TimeSpan.FromMilliseconds(800));
        await SessionReport.PrintStateAsync(spotify);
    }
}
