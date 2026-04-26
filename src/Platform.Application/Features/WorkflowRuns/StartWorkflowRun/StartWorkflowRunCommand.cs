namespace Platform.Application.Features.WorkflowRuns.StartWorkflowRun;

public sealed record StartWorkflowRunCommand(
    string Name,
    string WorkflowType,
    string? TaskQueue);
