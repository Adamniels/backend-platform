using System.Text.Json;
using Platform.Application.Abstractions.News;
using Platform.Application.Features.News.RankedFeed;
using Platform.Contracts.V1;

namespace Platform.Application.Features.News.ListFeed;

public sealed class ListNewsFeedQueryHandler(
    INewsReadRepository       news,
    INewsVectorSearch         vectorSearch,
    INewsRankedFeedRepository rankedFeedRepo)
{
    private const int FeedLimit = 30;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<NewsItemSummaryDto>> HandleAsync(
        ListNewsFeedQuery query,
        CancellationToken cancellationToken = default)
    {
        // Phase 5: serve the pre-computed LLM-ranked feed when available.
        var ranked = await rankedFeedRepo
            .GetAsync(query.UserId, cancellationToken)
            .ConfigureAwait(false);

        if (ranked is not null)
        {
            var entries = JsonSerializer.Deserialize<List<RankedFeedEntry>>(
                ranked.EntriesJson, JsonOptions) ?? [];

            if (entries.Count > 0)
            {
                // Deduplicate — Claude occasionally returns the same article ID twice.
                var unique   = entries.GroupBy(e => e.NewsItemId).Select(g => g.First()).ToList();
                var ids      = unique.Select(e => e.NewsItemId);
                var items    = await news.GetByIdsAsync(ids, cancellationToken).ConfigureAwait(false);
                var itemMap  = items.ToDictionary(x => x.Id);
                var scoreMap = unique.ToDictionary(e => e.NewsItemId, e => e.Score / 100.0);
                var exMap    = unique.ToDictionary(e => e.NewsItemId, e => e.Explanation);

                return unique
                    .Where(e => itemMap.ContainsKey(e.NewsItemId))
                    .Select(e => itemMap[e.NewsItemId] with
                    {
                        RelevanceScore       = scoreMap.GetValueOrDefault(e.NewsItemId),
                        RelevanceExplanation = exMap.GetValueOrDefault(e.NewsItemId),
                    })
                    .ToList();
            }
        }

        // Phase 2–4 fallback: vector search, no explanations.
        var hits = await vectorSearch
            .RankByRelevanceAsync(query.UserId, FeedLimit, cancellationToken)
            .ConfigureAwait(false);

        if (hits.Count == 0)
            return await news.ListFeedAsync(cancellationToken).ConfigureAwait(false);

        var hitScoreMap = hits.ToDictionary(h => h.NewsItemId, h => h.CosineSimilarity);
        var hitIds      = hits.Select(h => h.NewsItemId);
        var hitItems    = await news.GetByIdsAsync(hitIds, cancellationToken).ConfigureAwait(false);

        return hitItems
            .OrderByDescending(x => hitScoreMap.GetValueOrDefault(x.Id))
            .Select(x => x with { RelevanceScore = hitScoreMap.GetValueOrDefault(x.Id) })
            .ToList();
    }
}
