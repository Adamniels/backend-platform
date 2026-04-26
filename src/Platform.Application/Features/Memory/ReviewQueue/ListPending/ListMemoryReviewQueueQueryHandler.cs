using Platform.Application.Abstractions.Memory.Review;
using Platform.Application.Features.Memory.ReviewQueue;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Features.Memory.ReviewQueue.ListPending;

public sealed class ListMemoryReviewQueueQueryHandler(IMemoryReviewService reviews)
{
    public async Task<IReadOnlyList<MemoryReviewQueueItemV1Dto>> HandleAsync(
        ListMemoryReviewQueueQuery query,
        CancellationToken cancellationToken = default)
    {
        var id = query.UserId is 0 ? MemoryUser.DefaultId : query.UserId;
        var rows = await reviews
            .ListPendingAsync(id, cancellationToken)
            .ConfigureAwait(false);
        return MemoryReviewQueueItemMapper.ToV1Dtos(rows);
    }
}
