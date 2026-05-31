using Platform.Application.Abstractions.News;

namespace Platform.Application.Features.News.RankedFeed;

public sealed class GetNewsFeedCandidatesQueryHandler(
    INewsVectorSearch     vectorSearch,
    INewsReadRepository   newsRepo,
    IUserInterestProvider interests)
{
    private const int SnippetLength = 300;

    public async Task<NewsFeedCandidatesResult> HandleAsync(
        GetNewsFeedCandidatesQuery query,
        CancellationToken cancellationToken = default)
    {
        // Run Phase 4 vector search for the larger candidate pool.
        var hits = await vectorSearch
            .RankByRelevanceAsync(query.UserId, query.Limit, cancellationToken)
            .ConfigureAwait(false);

        // Load article metadata for the matched IDs.
        var items = hits.Count > 0
            ? await newsRepo.GetByIdsAsync(hits.Select(h => h.NewsItemId), cancellationToken).ConfigureAwait(false)
            : [];

        // Load user context so the worker can build a personalised LLM prompt.
        var snapshot = await interests
            .GetInterestsAsync(query.UserId, cancellationToken)
            .ConfigureAwait(false);

        var userContext = new UserInterestContextDto(
            snapshot.CoreInterests,
            snapshot.SecondaryInterests,
            snapshot.Goals,
            snapshot.ActiveProjects.Select(p => p.Name).ToList());

        // Preserve vector-ranked order; truncate body to a short snippet for the LLM prompt.
        var scoreOrder = hits.Select(h => h.NewsItemId).ToList();
        var itemMap    = items.ToDictionary(x => x.Id);

        var candidates = scoreOrder
            .Where(id => itemMap.ContainsKey(id))
            .Select(id =>
            {
                var item    = itemMap[id];
                var snippet = item.Body is { Length: > 0 } b
                    ? b[..Math.Min(SnippetLength, b.Length)]
                    : null;
                return new NewsFeedCandidateDto(id, item.Title, item.Source, item.PublishedAt, snippet);
            })
            .ToList();

        return new NewsFeedCandidatesResult(userContext, candidates);
    }
}
