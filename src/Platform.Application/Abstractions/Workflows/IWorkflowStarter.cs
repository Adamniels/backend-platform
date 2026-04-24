namespace Platform.Application.Abstractions.Workflows;

public interface IWorkflowStarter
{
    /// <summary>
    /// Starts a Temporal workflow for the given logical type and returns Temporal workflow id, or null if not configured.
    /// </summary>
    Task<string?> StartAsync(string temporalTaskQueue, string workflowType, string workflowRunId, CancellationToken cancellationToken = default);
}
