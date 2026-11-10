// StartupTaskPlan.cs: decides what to do with WinIsland's startup task when the "Launch with
// Windows" switch changes or WinIsland starts (PRD FR-20). Kept in Core so the rules have unit tests.
// Windows only lets an app turn its task on while the user has not turned it off in Windows, and
// never against a policy; in those cases nothing is changed and Settings shows why.

namespace WinIsland.Core.Startup;

public enum StartupTaskAction
{
    None,
    Enable,
    Disable,
}

/// <summary>What to do now, and the text to show when Windows will not follow the switch.</summary>
public sealed record StartupTaskDecision(StartupTaskAction Action, string? Problem);

public static class StartupTaskPlan
{
    public const string TurnedOffInWindowsText =
        "Windows has WinIsland turned off under Startup apps. Turn it on in Windows Settings > Apps > Startup.";

    public const string PolicyKeepsOffText = "A policy on this PC keeps WinIsland from starting with Windows.";

    public const string PolicyKeepsOnText = "A policy on this PC makes WinIsland start with Windows.";

    public static StartupTaskDecision Decide(bool launchAtStartup, StartupTaskStatus status)
    {
        return (launchAtStartup, status) switch
        {
            (true, StartupTaskStatus.Disabled) => new(StartupTaskAction.Enable, null),
            (true, StartupTaskStatus.DisabledByUser) => new(StartupTaskAction.None, TurnedOffInWindowsText),
            (true, StartupTaskStatus.DisabledByPolicy) => new(StartupTaskAction.None, PolicyKeepsOffText),
            (false, StartupTaskStatus.Enabled) => new(StartupTaskAction.Disable, null),
            (false, StartupTaskStatus.EnabledByPolicy) => new(StartupTaskAction.None, PolicyKeepsOnText),
            _ => new(StartupTaskAction.None, null),
        };
    }

    /// <summary>The text to show when the task did not end up as the switch says, or null when it did.</summary>
    public static string? ProblemAfter(bool launchAtStartup, StartupTaskStatus result)
    {
        return Decide(launchAtStartup, result) switch
        {
            { Problem: string problem } => problem,
            { Action: StartupTaskAction.None } => null,
            _ => launchAtStartup
                ? "Windows did not turn on starting WinIsland with Windows."
                : "Windows did not turn off starting WinIsland with Windows.",
        };
    }
}
