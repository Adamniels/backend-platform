using Platform.Contracts.V1;

namespace Platform.Application.Abstractions.News;

public interface INewsReadRepository
{
    Task<IReadOnlyList<NewsItemSummaryDto>> ListFeedAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns the raw body text of an article by ID, or null if not found.</summary>
    Task<string?> GetBodyByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Fetches a set of articles by their IDs, preserving the order of <paramref name="ids"/>.</summary>
    Task<IReadOnlyList<NewsItemSummaryDto>> GetByIdsAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default);
}
