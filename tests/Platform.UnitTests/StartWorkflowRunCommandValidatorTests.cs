using Platform.Application.Features.WorkflowRuns.StartWorkflowRun;

namespace Platform.UnitTests;

public sealed class StartWorkflowRunCommandValidatorTests
{
    [Fact]
    public void Empty_name_fails_validation()
    {
        var v = new StartWorkflowRunCommandValidator();
        var result = v.Validate(new StartWorkflowRunCommand("", "SomeWorkflow", null));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Valid_command_passes()
    {
        var v = new StartWorkflowRunCommandValidator();
        var result = v.Validate(new StartWorkflowRunCommand("Run 1", "SomeWorkflow", null));
        Assert.True(result.IsValid);
    }
}
