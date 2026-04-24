namespace Platform.Domain.Features.WorkflowRuns;

public sealed class WorkflowRun
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public WorkflowRunStatus Status { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string? TemporalWorkflowId { get; set; }
}
