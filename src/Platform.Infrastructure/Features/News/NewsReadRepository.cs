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
                // Prefer the Markdown summary; fall back to raw body.
                string.IsNullOrEmpty(x.SummaryBody) ? (string.IsNullOrEmpty(x.Body) ? null : x.Body) : x.SummaryBody,
                null))
            .ToListAsync(cancellationToken);

    public async Task<string?> GetBodyByIdAsync(string id, CancellationToken cancellationToken = default) =>
        await db.NewsItems.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => x.Body)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<(string Title, string Body)?> GetTitleAndBodyAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var item = await db.NewsItems.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new { x.Title, x.Body })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return item is null ? null : (item.Title, item.Body);
    }

    public async Task StoreSummaryAsync(
        string id,
        string summaryMarkdown,
        CancellationToken cancellationToken = default)
    {
        await db.NewsItems
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(
                s => s.SetProperty(x => x.SummaryBody, summaryMarkdown),
                cancellationToken)
            .ConfigureAwait(false);
    }

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
                // Prefer the Markdown summary; fall back to raw body.
                string.IsNullOrEmpty(x.SummaryBody) ? (string.IsNullOrEmpty(x.Body) ? null : x.Body) : x.SummaryBody,
                null))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
