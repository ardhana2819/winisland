// Program.cs: entry point of the NotificationProbe spike.
// Reports whether the process has package identity and notification access, then watches
// notifications two ways (event and polling) so we can compare which one actually works.
//
// Usage:
//   NotificationProbe.exe --check       Print identity and access status, then exit.
//   NotificationProbe.exe               Watch notifications, print app, ids, and text lengths.
//   NotificationProbe.exe --show-text   Same, and also print the message text (console only, U7).
// Message text is only ever written to this console window, never to a file.

using NotificationProbe;

bool checkOnly = args.Contains("--check");
bool showText = args.Contains("--show-text");
ConsoleReport report = new(showText);

report.Line($"NotificationProbe started. Show text: {(showText ? "yes" : "no")}.");
report.Line($"Package identity: {PackageIdentityCheck.Describe()}");

bool accessGranted = await ListenerAccess.CheckAndRequestAsync(report);

if (checkOnly || !accessGranted)
{
    return accessGranted ? 0 : 1;
}

NotificationTracker tracker = new(report);
NotificationEventWatcher eventWatcher = new(tracker, report);
eventWatcher.TryStart();

NotificationPoller poller = new(tracker, report);
using CancellationTokenSource stop = new();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    stop.Cancel();
};

report.Line("Watching notifications. Press Ctrl+C to stop.");
await poller.RunAsync(stop.Token);
report.Line("Stopped.");
return 0;
