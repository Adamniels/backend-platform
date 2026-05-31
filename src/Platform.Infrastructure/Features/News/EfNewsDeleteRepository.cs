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
            return 0;

        // Delete interaction history first — the FK on news_interactions.NewsItemId
        // is set to RESTRICT so Postgres blocks deleting an article that has interactions.
        // Interactions are removed alongside the article: if the user explicitly deletes
        // an article they no longer want it influencing their profile either.
        await db.NewsInteractions
            .Where(x => ids.Contains(x.NewsItemId))
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);

        return await db.NewsItems
            .Where(x => ids.Contains(x.Id))
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
