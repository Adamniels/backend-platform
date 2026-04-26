using Platform.Contracts.V1.Memory;

namespace Platform.Application.Abstractions.Memory.Review;

public interface IMemoryReviewQueueReadRepository
{
    Task<IReadOnlyList<MemoryReviewQueueItemV1Dto>> ListPendingForUserAsync(
        int userId,
        CancellationToken cancellationToken = default);
}
