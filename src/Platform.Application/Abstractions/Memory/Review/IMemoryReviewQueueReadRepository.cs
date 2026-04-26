using Platform.Contracts.V1.Memory;

namespace Platform.Application.Abstractions.Memory.Review;

public interface IMemoryReviewQueueReadRepository
{
    Task<IReadOnlyList<MemoryReviewQueueItemV1Dto>> ListPendingForPrincipalAsync(
        int principalId,
        CancellationToken cancellationToken = default);
}
