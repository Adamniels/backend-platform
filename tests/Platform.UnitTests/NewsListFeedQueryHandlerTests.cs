using Platform.Application.Abstractions.News;
using Platform.Application.Features.News.ListFeed;
using Platform.Contracts.V1;

namespace Platform.UnitTests;

public sealed class NewsListFeedQueryHandlerTests
{
    // ---------------------------------------------------------------------------
    // Test doubles
    // ---------------------------------------------------------------------------

    private sealed class StubVectorSearch(IReadOnlyList<NewsVectorHit> hits) : INewsVectorSearch
    {
        public Task<IReadOnlyList<NewsVectorHit>> RankByRelevanceAsync(
            int userId, int limit, CancellationToken cancellationToken = default) =>
            Task.FromResult(hits);
    }

    private sealed class StubNewsReadRepository(
        IReadOnlyList<NewsItemSummaryDto> feed,
        IReadOnlyList<NewsItemSummaryDto> byIds) : INewsReadRepository
    {
        public bool ListFeedCalled { get; private set; }

        public Task<IReadOnlyList<NewsItemSummaryDto>> ListFeedAsync(CancellationToken cancellationToken = default)
        {
            ListFeedCalled = true;
            return Task.FromResult(feed);
        }

        public Task<string?> GetBodyByIdAsync(string id, CancellationToken cancellationToken = default) =>
            Task.FromResult<string?>(null);

        public Task<IReadOnlyList<NewsItemSummaryDto>> GetByIdsAsync(
            IEnumerable<string> ids, CancellationToken cancellationToken = default) =>
            Task.FromResult(byIds);
    }

    // ---------------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------------

    private static NewsItemSummaryDto MakeDto(string id, double? score = null) =>
        new(id, "Title", "Source", "2025-01-01T00:00:00Z", null, null, score);

    // ---------------------------------------------------------------------------
    // D1 tests
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task When_vector_hits_exist_result_is_ordered_by_relevance_and_scores_are_populated()
    {
        var hits = new List<NewsVectorHit>
        {
            new("ni-aaa", 0.9),
            new("ni-bbb", 0.6),
            new("ni-ccc", 0.75),
        };

        // GetByIdsAsync returns items in an arbitrary order; handler must re-sort by score.
        IReadOnlyList<NewsItemSummaryDto> items = [MakeDto("ni-bbb"), MakeDto("ni-aaa"), MakeDto("ni-ccc")];

        var newsRepo = new StubNewsReadRepository([], items);
        var handler = new ListNewsFeedQueryHandler(newsRepo, new StubVectorSearch(hits));

        var result = await handler.HandleAsync(new ListNewsFeedQuery(UserId: 1));

        Assert.Equal(3, result.Count);
        Assert.Equal("ni-aaa", result[0].Id);
        Assert.Equal(0.9, result[0].RelevanceScore);
        Assert.Equal("ni-ccc", result[1].Id);
        Assert.Equal(0.75, result[1].RelevanceScore);
        Assert.Equal("ni-bbb", result[2].Id);
        Assert.Equal(0.6, result[2].RelevanceScore);

        Assert.False(newsRepo.ListFeedCalled, "Chronological fallback must not be called when hits are returned.");
    }

    [Fact]
    public async Task When_vector_search_returns_empty_falls_back_to_chronological()
    {
        IReadOnlyList<NewsItemSummaryDto> chronological = [MakeDto("ni-111"), MakeDto("ni-222")];
        var newsRepo = new StubNewsReadRepository(chronological, []);
        var handler = new ListNewsFeedQueryHandler(newsRepo, new StubVectorSearch([]));

        var result = await handler.HandleAsync(new ListNewsFeedQuery(UserId: 1));

        Assert.Equal(2, result.Count);
        Assert.Equal("ni-111", result[0].Id);
        Assert.True(newsRepo.ListFeedCalled, "ListFeedAsync must be called when there is no profile.");
        Assert.All(result, x => Assert.Null(x.RelevanceScore));
    }
}
