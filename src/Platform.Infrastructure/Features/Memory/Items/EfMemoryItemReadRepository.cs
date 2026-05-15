using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions.Memory.Items;
using Platform.Contracts.V1.Memory;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.Memory.Items;

public sealed class EfMemoryItemReadRepository(PlatformDbContext db) : IMemoryItemReadRepository
{
    public async Task<IReadOnlyList<MemoryItemSummaryV1Dto>> ListSummariesForUserAsync(
        int userId,
        CancellationToken cancellationToken = default) =>
        await db.MemoryItems
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new MemoryItemSummaryV1Dto(
                x.Id,
                x.Title,
                x.MemoryType.ToString(),
                x.Status.ToString()))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
}
