using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions.Memory.Legacy;
using Platform.Contracts.V1;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.Memory.Legacy;

public sealed class LegacyMemoryInsightsReadRepository(PlatformDbContext db) : ILegacyMemoryInsightsReadRepository
{
    public async Task<IReadOnlyList<MemoryInsightDto>> ListInsightsAsync(
        CancellationToken cancellationToken = default) =>
        await db.MemoryInsights
            .AsNoTracking()
            .Select(x => new MemoryInsightDto(x.Id, x.Label, x.Content, x.Strength, x.Confirmed))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
}
