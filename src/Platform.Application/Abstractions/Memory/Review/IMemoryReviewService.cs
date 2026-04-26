using Platform.Domain.Features.Memory.Entities;

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

    /// <summary>Used by consolidation idempotency: pending item with the same fingerprint in <c>EvidenceJson</c>.</summary>
    Task<bool> HasPendingWithEvidenceSubstringAsync(
        int userId,
        string evidenceSubstring,
        CancellationToken cancellationToken = default);
}
