using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions.WorkflowRuns;
using Platform.Contracts.V1;
using Platform.Domain.Features.WorkflowRuns;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.WorkflowRuns;

public sealed class WorkflowRunRepository(PlatformDbContext db) : IWorkflowRunRepository
{
    public async Task<IReadOnlyList<WorkflowRunSummaryDto>> ListSummariesAsync(
        CancellationToken cancellationToken = default)
    {
        return await db.WorkflowRuns.AsNoTracking()
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new WorkflowRunSummaryDto(
                x.Id,
                x.Name,
                WorkflowRunStatusMapper.ToApiString(x.Status),
                x.UpdatedAt.ToString("O")))
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkflowRun> AddPendingAsync(
        string name,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        var id = $"wr-{Guid.NewGuid():N}";
        var entity = new WorkflowRun
        {
            Id = id,
            Name = name,
            Status = WorkflowRunStatus.Pending,
            UpdatedAt = now,
            TemporalWorkflowId = null,
        };
        db.WorkflowRuns.Add(entity);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity;
    }

    public async Task SaveRunAfterTemporalStartAsync(WorkflowRun run, CancellationToken cancellationToken = default)
    {
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
