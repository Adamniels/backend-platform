using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions.News;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.News;

public sealed class EfNewsDeleteRepository(PlatformDbContext db) : INewsDeleteRepository
{
    public async Task<int> DeleteByIdsAsync(
        IReadOnlyList<string> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return 0;
        }

        return await db.NewsItems
            .Where(x => ids.Contains(x.Id))
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
