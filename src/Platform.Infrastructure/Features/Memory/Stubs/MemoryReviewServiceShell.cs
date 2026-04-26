using Platform.Application.Abstractions.Memory.Review;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Infrastructure.Features.Memory.Stubs;

public sealed class MemoryReviewServiceShell : IMemoryReviewService
{
    public Task<IReadOnlyList<MemoryReviewQueueItem>> ListPendingAsync(
        int _,
        CancellationToken __ = default) =>
        Task.FromResult<IReadOnlyList<MemoryReviewQueueItem>>(
            Array.Empty<MemoryReviewQueueItem>());

    public Task ApproveAsync(long _1, int _2, CancellationToken _3 = default) =>
        Task.FromException(
            new NotSupportedException(
                "Review persistence is not wired. Implement IMemoryReviewService in Infrastructure."));

    public Task RejectAsync(long _1, int _2, CancellationToken _3 = default) =>
        Task.FromException(
            new NotSupportedException(
                "Review persistence is not wired. Implement IMemoryReviewService in Infrastructure."));
}
