// StartupTaskPlanTests.cs: checks what WinIsland does with its Windows startup task for each
// switch position and task state (PRD FR-20), and which text Settings shows when Windows will not
// follow the switch.

using WinIsland.Core.Startup;

namespace WinIsland.Core.Tests.Startup;

public class StartupTaskPlanTests
{
    [Fact]
    public void Switching_on_enables_a_task_that_is_off()
    {
        Assert.Equal(new StartupTaskDecision(StartupTaskAction.Enable, null), StartupTaskPlan.Decide(true, StartupTaskStatus.Disabled));
    }

    [Fact]
    public void Switching_off_disables_a_task_that_is_on()
    {
        Assert.Equal(new StartupTaskDecision(StartupTaskAction.Disable, null), StartupTaskPlan.Decide(false, StartupTaskStatus.Enabled));
    }

    [Theory]
    [InlineData(true, StartupTaskStatus.Enabled)]
    [InlineData(true, StartupTaskStatus.EnabledByPolicy)]
    [InlineData(false, StartupTaskStatus.Disabled)]
    [InlineData(false, StartupTaskStatus.DisabledByUser)]
    [InlineData(false, StartupTaskStatus.DisabledByPolicy)]
    public void Nothing_changes_when_the_task_already_matches(bool launchAtStartup, StartupTaskStatus status)
    {
        Assert.Equal(new StartupTaskDecision(StartupTaskAction.None, null), StartupTaskPlan.Decide(launchAtStartup, status));
    }

    [Fact]
    public void Switching_on_after_the_user_turned_it_off_in_windows_explains_where_to_turn_it_on()
    {
        Assert.Equal(
            new StartupTaskDecision(StartupTaskAction.None, StartupTaskPlan.TurnedOffInWindowsText),
            StartupTaskPlan.Decide(true, StartupTaskStatus.DisabledByUser));
    }

    [Theory]
    [InlineData(true, StartupTaskStatus.DisabledByPolicy, StartupTaskPlan.PolicyKeepsOffText)]
    [InlineData(false, StartupTaskStatus.EnabledByPolicy, StartupTaskPlan.PolicyKeepsOnText)]
    public void A_policy_is_never_fought_and_is_explained(bool launchAtStartup, StartupTaskStatus status, string expected)
    {
        Assert.Equal(new StartupTaskDecision(StartupTaskAction.None, expected), StartupTaskPlan.Decide(launchAtStartup, status));
    }

    [Theory]
    [InlineData(true, StartupTaskStatus.Enabled)]
    [InlineData(false, StartupTaskStatus.Disabled)]
    public void No_problem_when_the_result_matches_the_switch(bool launchAtStartup, StartupTaskStatus result)
    {
        Assert.Null(StartupTaskPlan.ProblemAfter(launchAtStartup, result));
    }

    [Fact]
    public void Enabling_refused_by_the_user_setting_shows_where_to_turn_it_on()
    {
        Assert.Equal(StartupTaskPlan.TurnedOffInWindowsText, StartupTaskPlan.ProblemAfter(true, StartupTaskStatus.DisabledByUser));
    }

    [Fact]
    public void A_task_still_off_after_enabling_is_reported()
    {
        Assert.Equal("Windows did not turn on starting WinIsland with Windows.", StartupTaskPlan.ProblemAfter(true, StartupTaskStatus.Disabled));
    }

    [Fact]
    public void A_task_still_on_after_disabling_is_reported()
    {
        Assert.Equal("Windows did not turn off starting WinIsland with Windows.", StartupTaskPlan.ProblemAfter(false, StartupTaskStatus.Enabled));
    }
}
