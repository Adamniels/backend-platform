using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Abstractions.Memory.Review;

/// <summary>User decision surface for the review queue (approve / reject / supersede) per master non-negotiables.</summary>
public interface IMemoryReviewService
{
    Task<IReadOnlyList<MemoryReviewQueueItem>> ListPendingAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task ApproveAsync(
        long reviewItemId,
        int userId,
        CancellationToken cancellationToken = default);

    Task RejectAsync(
        long reviewItemId,
        int userId,
        CancellationToken cancellationToken = default);
}
