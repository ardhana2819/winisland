// ConsoleReport.cs: prints spike output with timestamps, to the console only.
// Message text is printed only when --show-text is given. Nothing here writes to a file.

namespace NotificationProbe;

internal sealed class ConsoleReport(bool showText)
{
    private readonly Lock _gate = new();

    public void Line(string message)
    {
        lock (_gate)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
        }
    }

    public void Error(string what, Exception error)
    {
        Line($"ERROR {what}: {error.GetType().Name}, HRESULT 0x{error.HResult:X8}, {error.Message}");
    }

    public void Added(NotificationSnapshot snapshot, string seenVia)
    {
        // Delay = time from Windows creating the notification until we saw it (goal G2: under 2 s).
        double delaySeconds = (DateTimeOffset.Now - snapshot.CreatedAt).TotalSeconds;
        Describe("ADDED  ", snapshot, seenVia, $" delay={delaySeconds:F1}s");
    }

    public void Updated(NotificationSnapshot snapshot, string seenVia)
    {
        Describe("UPDATED", snapshot, seenVia, string.Empty);
    }

    public void Removed(NotificationSnapshot snapshot, string seenVia)
    {
        Line($"REMOVED id={snapshot.Id} app=\"{snapshot.AppName}\" via={seenVia}");
    }

    private void Describe(string kind, NotificationSnapshot snapshot, string seenVia, string extra)
    {
        string lengths = string.Join(",", snapshot.Texts.Select(text => text.Length));
        lock (_gate)
        {
            Line($"{kind} id={snapshot.Id} app=\"{snapshot.AppName}\" aumid=\"{snapshot.AppUserModelId}\" " +
                 $"via={seenVia}{extra} textCount={snapshot.Texts.Count} textLengths=[{lengths}]");

            if (showText)
            {
                for (int index = 0; index < snapshot.Texts.Count; index++)
                {
                    Console.WriteLine($"           text[{index}]: {snapshot.Texts[index]}");
                }
            }
        }
    }
}
