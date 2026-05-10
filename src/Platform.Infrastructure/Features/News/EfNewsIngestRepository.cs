using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions.News;
using Platform.Domain.Features.News;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.News;

public sealed class EfNewsIngestRepository(PlatformDbContext db) : INewsIngestRepository
{
    public async Task<(bool Created, string Id)> TryInsertAsync(
        NewsItem item,
        string urlHash,
        CancellationToken cancellationToken = default)
    {
        var exists = await db.NewsItems.AsNoTracking()
            .AnyAsync(x => x.UrlHash == urlHash, cancellationToken)
            .ConfigureAwait(false);
        if (exists)
        {
            return (false, item.Id);
        }

        item.UrlHash = urlHash;
        db.NewsItems.Add(item);
        try
        {
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return (true, item.Id);
        }
        catch (DbUpdateException)
        {
            db.Entry(item).State = EntityState.Detached;
            return (false, item.Id);
        }
    }
}
