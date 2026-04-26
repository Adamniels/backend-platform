using Platform.Application.Abstractions.News;
using Platform.Contracts.V1;

namespace Platform.Application.Features.News.ListFeed;

public sealed class ListNewsFeedQueryHandler(INewsReadRepository news)
{
    public async Task<IReadOnlyList<NewsItemSummaryDto>> HandleAsync(
        ListNewsFeedQuery _,
        CancellationToken cancellationToken = default) =>
        await news.ListFeedAsync(cancellationToken).ConfigureAwait(false);
}
