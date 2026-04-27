using Platform.Application.Abstractions.Memory.Review;
using Platform.Application.Abstractions.Memory.Users;
using Platform.Application.Features.Memory.ReviewQueue;
using Platform.Contracts.V1.Memory;

namespace Platform.Application.Features.Memory.ReviewQueue.ListPending;

public sealed class ListMemoryReviewQueueQueryHandler(
    IMemoryReviewService reviews,
    IMemoryUserContextResolver userResolver)
{
    public async Task<IReadOnlyList<MemoryReviewQueueItemV1Dto>> HandleAsync(
        ListMemoryReviewQueueQuery query,
        CancellationToken cancellationToken = default)
    {
        var id = userResolver.Resolve(query.UserId);
        var rows = await reviews
            .ListPendingAsync(id, cancellationToken)
            .ConfigureAwait(false);
        return MemoryReviewQueueItemMapper.ToV1Dtos(rows);
    }
}
