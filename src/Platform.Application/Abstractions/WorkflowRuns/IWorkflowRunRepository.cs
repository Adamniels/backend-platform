using Platform.Contracts.V1;
using Platform.Domain.Features.WorkflowRuns;

namespace Platform.Application.Abstractions.WorkflowRuns;

public interface IWorkflowRunRepository
{
    Task<IReadOnlyList<WorkflowRunSummaryDto>> ListSummariesAsync(
        CancellationToken cancellationToken = default);

    Task<WorkflowRun> AddPendingAsync(
        string name,
        DateTimeOffset now,
        CancellationToken cancellationToken = default);

    /// <summary>Persists changes on the tracked <paramref name="run"/> (status, Temporal id, timestamp) after Temporal start.</summary>
    Task SaveRunAfterTemporalStartAsync(WorkflowRun run, CancellationToken cancellationToken = default);
}
