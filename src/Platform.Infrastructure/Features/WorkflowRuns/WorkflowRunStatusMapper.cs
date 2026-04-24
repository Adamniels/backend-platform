using Platform.Domain.Features.WorkflowRuns;

namespace Platform.Infrastructure.Features.WorkflowRuns;

internal static class WorkflowRunStatusMapper
{
    public static string ToApiString(WorkflowRunStatus status) => status switch
    {
        WorkflowRunStatus.Pending => "pending",
        WorkflowRunStatus.Running => "running",
        WorkflowRunStatus.NeedsInput => "needs_input",
        WorkflowRunStatus.Completed => "completed",
        WorkflowRunStatus.Failed => "failed",
        _ => "pending",
    };
}
