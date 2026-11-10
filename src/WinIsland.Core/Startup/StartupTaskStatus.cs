// StartupTaskStatus.cs: the states Windows reports for WinIsland's startup task (PRD FR-20).
// With package identity, Windows itself starts WinIsland at sign-in through this task; the user
// can also turn it on or off under Settings > Apps > Startup. Mirrors Windows' own
// StartupTaskState, so Core can decide what to do without referencing Windows APIs.

namespace WinIsland.Core.Startup;

public enum StartupTaskStatus
{
    /// <summary>Off, and WinIsland may turn it on.</summary>
    Disabled,

    /// <summary>On: Windows starts WinIsland at sign-in.</summary>
    Enabled,

    /// <summary>The user turned it off in Windows; only the user can turn it on again there.</summary>
    DisabledByUser,

    /// <summary>A policy on this PC keeps it off.</summary>
    DisabledByPolicy,

    /// <summary>A policy on this PC keeps it on.</summary>
    EnabledByPolicy,
}
