using Platform.Application.Abstractions.Memory.Review;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Features.Memory.ReviewQueue.ListPending;

public sealed class ListMemoryReviewQueueQueryHandler(IMemoryReviewQueueReadRepository review)
{
    public async Task<IReadOnlyList<MemoryReviewQueueItemV1Dto>> HandleAsync(
        ListMemoryReviewQueueQuery query,
        CancellationToken cancellationToken = default)
    {
        var id = query.PrincipalId is 0 ? MemoryPrincipal.SingleTenantKey : query.PrincipalId;
        return await review
            .ListPendingForPrincipalAsync(id, cancellationToken)
            .ConfigureAwait(false);
    }
}
