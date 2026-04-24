using Microsoft.EntityFrameworkCore;
using Platform.Application.Features.News;
using Platform.Contracts.V1;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.News;

public sealed class NewsQueries(PlatformDbContext db) : INewsQueries
{
    public async Task<IReadOnlyList<NewsItemSummaryDto>> ListFeedAsync(CancellationToken cancellationToken = default)
    {
        return await db.NewsItems.AsNoTracking()
            .OrderByDescending(x => x.PublishedAt)
            .Select(x => new NewsItemSummaryDto(x.Id, x.Title, x.Source, x.PublishedAt.ToString("O")))
            .ToListAsync(cancellationToken);
    }
}
