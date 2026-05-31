using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions.News;
using Platform.Domain.Features.News;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.News;

public sealed class EfNewsRankedFeedRepository(PlatformDbContext db) : INewsRankedFeedRepository
{
    public async Task<NewsRankedFeed?> GetAsync(int userId, CancellationToken cancellationToken = default) =>
        await db.NewsRankedFeeds
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.UserId == userId, cancellationToken)
            .ConfigureAwait(false);

    public async Task UpsertAsync(NewsRankedFeed feed, CancellationToken cancellationToken = default)
    {
        var existing = await db.NewsRankedFeeds
            .FirstOrDefaultAsync(f => f.UserId == feed.UserId, cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            existing.EntriesJson = feed.EntriesJson;
            existing.ModelUsed   = feed.ModelUsed;
            existing.RankedAt    = feed.RankedAt;
        }
        else
        {
            db.NewsRankedFeeds.Add(feed);
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
