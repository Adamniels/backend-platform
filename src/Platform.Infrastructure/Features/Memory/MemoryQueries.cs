using Microsoft.EntityFrameworkCore;
using Platform.Application.Features.Memory;
using Platform.Contracts.V1;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.Memory;

public sealed class MemoryQueries(PlatformDbContext db) : IMemoryQueries
{
    public async Task<IReadOnlyList<MemoryInsightDto>> ListInsightsAsync(CancellationToken cancellationToken = default)
    {
        return await db.MemoryInsights.AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => new MemoryInsightDto(x.Id, x.Label, x.Content, x.Strength, x.Confirmed))
            .ToListAsync(cancellationToken);
    }
}
