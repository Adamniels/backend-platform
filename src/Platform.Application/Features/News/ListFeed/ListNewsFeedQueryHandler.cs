using Platform.Application.Abstractions.News;
using Platform.Contracts.V1;

namespace Platform.Application.Features.News.ListFeed;

public sealed class ListNewsFeedQueryHandler(
    INewsReadRepository news,
    INewsVectorSearch vectorSearch)
{
    private const int FeedLimit = 30;

    public async Task<IReadOnlyList<NewsItemSummaryDto>> HandleAsync(
        ListNewsFeedQuery query,
        CancellationToken cancellationToken = default)
    {
        var hits = await vectorSearch
            .RankByRelevanceAsync(query.UserId, FeedLimit, cancellationToken)
            .ConfigureAwait(false);

        // No profile yet — fall back to chronological order.
        if (hits.Count == 0)
            return await news.ListFeedAsync(cancellationToken).ConfigureAwait(false);

        var scoreMap = hits.ToDictionary(h => h.NewsItemId, h => h.CosineSimilarity);
        var ids = hits.Select(h => h.NewsItemId);
        var items = await news.GetByIdsAsync(ids, cancellationToken).ConfigureAwait(false);

        return items
            .OrderByDescending(x => scoreMap.GetValueOrDefault(x.Id))
            .Select(x => x with { RelevanceScore = scoreMap.GetValueOrDefault(x.Id) })
            .ToList();
    }
}
