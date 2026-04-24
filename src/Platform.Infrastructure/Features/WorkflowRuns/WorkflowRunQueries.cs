using Microsoft.EntityFrameworkCore;
using Platform.Application.Features.WorkflowRuns;
using Platform.Contracts.V1;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.WorkflowRuns;

public sealed class WorkflowRunQueries(PlatformDbContext db) : IWorkflowRunQueries
{
    public async Task<IReadOnlyList<WorkflowRunSummaryDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var rows = await db.WorkflowRuns.AsNoTracking()
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new WorkflowRunSummaryDto(
                x.Id,
                x.Name,
                WorkflowRunStatusMapper.ToApiString(x.Status),
                x.UpdatedAt.ToString("O")))
            .ToListAsync(cancellationToken);
        return rows;
    }
}
