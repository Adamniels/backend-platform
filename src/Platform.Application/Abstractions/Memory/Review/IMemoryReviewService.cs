using Platform.Domain.Features.Memory.Entities;
using Platform.Domain.Features.Memory;

namespace Platform.Application.Abstractions.Memory.Review;

/// <summary>Review queue writes and decisions (approve / reject / edit pending).</summary>
public interface IMemoryReviewService
{
    Task<MemoryReviewQueueItem> CreatePendingAsync(
        MemoryReviewQueueItem item,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MemoryReviewQueueItem>> ListPendingAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<MemoryReviewQueueItem?> GetByIdForUserAsync(
        long reviewItemId,
        int userId,
        CancellationToken cancellationToken = default);

    Task<MemoryReviewApprovalResult> ApproveAsync(
        long reviewItemId,
        int userId,
        string? reviewNotes,
        CancellationToken cancellationToken = default);

    Task RejectAsync(
        long reviewItemId,
        int userId,
        string? reason,
        CancellationToken cancellationToken = default);

    Task UpdatePendingAsync(
        long reviewItemId,
        int userId,
        string? title,
        string? summary,
        string? proposedChangeJson,
        CancellationToken cancellationToken = default);

    Task<bool> HasPendingWithFingerprintAsync(
        int userId,
        MemoryReviewProposalType proposalType,
        string dedupFingerprint,
        CancellationToken cancellationToken = default);
}
