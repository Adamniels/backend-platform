using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions.News;
using Platform.Contracts.V1;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.News;

public sealed class NewsReadRepository(PlatformDbContext db) : INewsReadRepository
{
    public async Task<IReadOnlyList<NewsItemSummaryDto>> ListFeedAsync(CancellationToken cancellationToken = default) =>
        await db.NewsItems.AsNoTracking()
            .OrderByDescending(x => x.PublishedAt)
            .Take(30)
            .Select(x => new NewsItemSummaryDto(
                x.Id,
                x.Title,
                x.Source,
                x.PublishedAt.ToString("O"),
                string.IsNullOrEmpty(x.Url) ? null : x.Url,
                string.IsNullOrEmpty(x.Body) ? null : x.Body,
                null))
            .ToListAsync(cancellationToken);

    public async Task<string?> GetBodyByIdAsync(string id, CancellationToken cancellationToken = default) =>
        await db.NewsItems.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => x.Body)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<NewsItemSummaryDto>> GetByIdsAsync(
        IEnumerable<string> ids,
        CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        return await db.NewsItems.AsNoTracking()
            .Where(x => idList.Contains(x.Id))
            .Select(x => new NewsItemSummaryDto(
                x.Id,
                x.Title,
                x.Source,
                x.PublishedAt.ToString("O"),
                string.IsNullOrEmpty(x.Url) ? null : x.Url,
                string.IsNullOrEmpty(x.Body) ? null : x.Body,
                null))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
