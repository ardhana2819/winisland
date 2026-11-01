// ConsoleReport.cs: prints spike output with timestamps, to the console only.
// Message text is printed only when --show-text is given. Nothing here writes to a file.

namespace NotificationProbe;

internal sealed class ConsoleReport(bool showText)
{
    // A bare site address such as web.whatsapp.com: letters, digits, dots and dashes, no spaces.
    private static readonly System.Text.RegularExpressions.Regex SiteAddress =
        new(@"^[a-z0-9-]+(\.[a-z0-9-]+)+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

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

                return;
            }

            // v1.2 spike (WhatsApp Web in Chrome): without --show-text, only lines that are a site
            // address (like web.whatsapp.com) are printed in full; a line that merely mentions
            // WhatsApp is flagged without its text. Senders and messages are never printed.
            for (int index = 0; index < snapshot.Texts.Count; index++)
            {
                string text = snapshot.Texts[index].Trim();
                if (SiteAddress.IsMatch(text))
                {
                    Console.WriteLine($"           text[{index}] is a site address: {text}");
                }
                else if (text.Contains("whatsapp", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"           text[{index}] mentions WhatsApp (text not shown)");
                }
            }
        }
    }
}
