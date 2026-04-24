namespace Platform.Domain.Features.WorkflowRuns;

public enum WorkflowRunStatus
{
    Pending = 0,
    Running = 1,
    NeedsInput = 2,
    Completed = 3,
    Failed = 4,
}
