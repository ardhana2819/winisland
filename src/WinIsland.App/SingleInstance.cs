// SingleInstance.cs: makes sure only one WinIsland runs at a time (PRD FR-6b).
// A second copy would show a second island and fail to get the hotkeys, so it closes at once.
// "Local\" keeps the check per Windows user session.

namespace WinIsland.App;

internal sealed class SingleInstance : IDisposable
{
    private const string MutexName = @"Local\WinIsland.SingleInstance";

    private readonly Mutex _mutex;

    public SingleInstance()
    {
        _mutex = new Mutex(initiallyOwned: true, MutexName, out bool createdNew);
        IsFirstInstance = createdNew;
    }

    public bool IsFirstInstance { get; }

    public void Dispose()
    {
        if (IsFirstInstance)
        {
            _mutex.ReleaseMutex();
        }

        _mutex.Dispose();
    }
}
