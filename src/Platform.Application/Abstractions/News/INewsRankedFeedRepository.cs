using Platform.Domain.Features.News;

namespace Platform.Application.Abstractions.News;

public interface INewsRankedFeedRepository
{
    /// <summary>Returns the current pre-computed ranking for a user, or null if none exists yet.</summary>
    Task<NewsRankedFeed?> GetAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>Inserts or fully replaces the ranked feed for the given user.</summary>
    Task UpsertAsync(NewsRankedFeed feed, CancellationToken cancellationToken = default);
}
