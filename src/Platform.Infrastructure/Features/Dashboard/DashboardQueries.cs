using Microsoft.EntityFrameworkCore;
using Platform.Application.Features.Dashboard;
using Platform.Contracts.V1;
using Platform.Domain.Features.WorkflowRuns;
using Platform.Infrastructure.Features.WorkflowRuns;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.Dashboard;

public sealed class DashboardQueries(PlatformDbContext db) : IDashboardQueries
{
    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var profile = await db.Profiles.AsNoTracking().SingleAsync(cancellationToken);
        var activeRuns = await db.WorkflowRuns.AsNoTracking()
            .CountAsync(x => x.Status == WorkflowRunStatus.Running, cancellationToken);
        var itemsNeedingAttention = await db.InputNeededItems.AsNoTracking().CountAsync(cancellationToken);
        var greeting = string.IsNullOrWhiteSpace(profile.DisplayName)
            ? "Welcome back"
            : $"Welcome back, {profile.DisplayName}";
        return new DashboardSummaryDto(greeting, activeRuns, itemsNeedingAttention);
    }
}
