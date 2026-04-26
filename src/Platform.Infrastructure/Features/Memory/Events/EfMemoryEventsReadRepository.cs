using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions.Memory.Events;
using Platform.Domain.Features.Memory.Entities;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.Memory.Events;

public sealed class EfMemoryEventsReadRepository(PlatformDbContext db) : IMemoryEventsReadRepository
{
    public async Task<IReadOnlyList<MemoryEvent>> ListOccurredInRangeAsync(
        int userId,
        DateTimeOffset startInclusive,
        DateTimeOffset endExclusive,
        int maxTake,
        CancellationToken cancellationToken = default)
    {
        var take = Math.Clamp(maxTake, 1, 100_000);
        return await db.MemoryEvents
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.OccurredAt >= startInclusive && e.OccurredAt < endExclusive)
            .OrderBy(e => e.OccurredAt)
            .ThenBy(e => e.Id)
            .Take(take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
