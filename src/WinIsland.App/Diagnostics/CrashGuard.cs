// CrashGuard.cs: catches errors nobody else caught, so one bug does not take the island away
// in the middle of a game (PRD section 7: no crash; recover when something drops).
// Each error is written to the log as its type, error code and code location only: the error's
// message text is left out on purpose, in case it ever carried data (section 14).
// Errors on the UI thread are marked handled so the island keeps running; errors elsewhere are
// logged before Windows ends the process.

using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace WinIsland.App.Diagnostics;

internal static class CrashGuard
{
    public static void Install(Dispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);

        dispatcher.UnhandledException += (_, e) =>
        {
            Record("Unhandled error on the UI thread (island keeps running)", e.Exception);
            e.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception error)
            {
                Record("Unhandled error, WinIsland is closing", error);
            }
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Record("Unobserved background error", e.Exception);
            e.SetObserved();
        };
    }

    private static void Record(string what, Exception error)
    {
        Exception innermost = error.GetBaseException();
        Trace.TraceError(
            "{0}: {1} (HRESULT 0x{2:X8}){3}{4}",
            what,
            innermost.GetType().FullName,
            innermost.HResult,
            Environment.NewLine,
            innermost.StackTrace ?? "(no stack trace)");
        Trace.Flush();
    }
}
