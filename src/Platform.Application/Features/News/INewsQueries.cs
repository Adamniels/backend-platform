using Platform.Contracts.V1;

namespace Platform.Application.Features.News;

public interface INewsQueries
{
    Task<IReadOnlyList<NewsItemSummaryDto>> ListFeedAsync(CancellationToken cancellationToken = default);
}
