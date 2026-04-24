using Platform.Contracts.V1;

namespace Platform.Application.Features.WorkflowRuns;

public interface IWorkflowRunQueries
{
    Task<IReadOnlyList<WorkflowRunSummaryDto>> ListAsync(CancellationToken cancellationToken = default);
}
