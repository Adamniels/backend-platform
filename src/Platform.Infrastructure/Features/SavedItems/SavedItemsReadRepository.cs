using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions.SavedItems;
using Platform.Contracts.V1;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.SavedItems;

public sealed class SavedItemsReadRepository(PlatformDbContext db) : ISavedItemsReadRepository
{
    public async Task<IReadOnlyList<SavedItemSummaryDto>> ListAsync(CancellationToken cancellationToken = default) =>
        await db.SavedItems.AsNoTracking()
            .OrderByDescending(x => x.SavedAt)
            .Select(x => new SavedItemSummaryDto(x.Id, x.Title, x.Kind, x.SavedAt.ToString("O")))
            .ToListAsync(cancellationToken);
}
