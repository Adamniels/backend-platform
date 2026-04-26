using Platform.Contracts.V1;

namespace Platform.Application.Abstractions.News;

public interface INewsReadRepository
{
    Task<IReadOnlyList<NewsItemSummaryDto>> ListFeedAsync(CancellationToken cancellationToken = default);
}
