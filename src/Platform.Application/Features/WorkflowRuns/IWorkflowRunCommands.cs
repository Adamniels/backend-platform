using Platform.Contracts.V1;

namespace Platform.Application.Features.WorkflowRuns;

public interface IWorkflowRunCommands
{
    Task<WorkflowRunSummaryDto> StartAsync(
        string name,
        string temporalWorkflowType,
        string temporalTaskQueue,
        CancellationToken cancellationToken = default);
}
