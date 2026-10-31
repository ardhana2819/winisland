// ErrorLogFile.cs: writes WinIsland's diagnostics to a small file, so problems can be found later.
// PRD section 14: logs hold event types and errors only, never message text. Every log line in
// WinIsland is written by its own code with a fixed wording plus error types and codes; no song
// title, sender or message is ever passed to the log.
// The file is %USERPROFILE%\.winisland\winisland.log, next to the settings. When it grows past
// 256 KB it is renamed to winisland.old.log and a new one starts, so it never grows without limit.

using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace WinIsland.App.Diagnostics;

internal sealed class ErrorLogFile : TraceListener
{
    private const long MaxBytes = 256 * 1024;

    private readonly string _path;
    private readonly string _oldPath;
    private readonly Lock _gate = new();

    public ErrorLogFile(string folder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(folder);
        Directory.CreateDirectory(folder);
        _path = Path.Combine(folder, "winisland.log");
        _oldPath = Path.Combine(folder, "winisland.old.log");
    }

    public override void Write(string? message) => Append(message, endLine: false);

    public override void WriteLine(string? message) => Append(message, endLine: true);

    // Trace.TraceError, TraceWarning and TraceInformation arrive here: one line per event, with its level.
    public override void TraceEvent(TraceEventCache? eventCache, string source, TraceEventType eventType, int id, string? format, params object?[]? args)
    {
        string text = args is { Length: > 0 } && format is not null
            ? string.Format(CultureInfo.InvariantCulture, format, args)
            : format ?? string.Empty;
        Append($"{eventType}: {text}", endLine: true);
    }

    public override void TraceEvent(TraceEventCache? eventCache, string source, TraceEventType eventType, int id, string? message)
    {
        Append($"{eventType}: {message}", endLine: true);
    }

    private void Append(string? message, bool endLine)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        lock (_gate)
        {
            try
            {
                RollIfTooBig();
                string stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                File.AppendAllText(_path, endLine ? $"{stamp} {message}{Environment.NewLine}" : message);
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException)
            {
                // The log is a helper; if the disk refuses, WinIsland keeps running without it.
            }
        }
    }

    private void RollIfTooBig()
    {
        FileInfo current = new(_path);
        if (current.Exists && current.Length > MaxBytes)
        {
            File.Move(_path, _oldPath, overwrite: true);
        }
    }
}
