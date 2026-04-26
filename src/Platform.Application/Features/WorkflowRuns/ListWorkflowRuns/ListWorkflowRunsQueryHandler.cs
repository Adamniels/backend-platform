using Platform.Application.Abstractions.WorkflowRuns;
using Platform.Contracts.V1;

namespace Platform.Application.Features.WorkflowRuns.ListWorkflowRuns;

public sealed class ListWorkflowRunsQueryHandler(IWorkflowRunRepository repository)
{
    public async Task<IReadOnlyList<WorkflowRunSummaryDto>> HandleAsync(
        ListWorkflowRunsQuery _,
        CancellationToken cancellationToken = default) =>
        await repository.ListSummariesAsync(cancellationToken).ConfigureAwait(false);
}
