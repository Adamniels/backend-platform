using Platform.Application.Abstractions.Memory.Review;
using Platform.Application.Features.Memory.Review;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Infrastructure.Features.Memory.Review.Approval;

internal sealed class ContradictionDetectedApprovalHandler : IMemoryReviewApprovalHandler
{
    public MemoryReviewProposalType ProposalType => MemoryReviewProposalType.ContradictionDetected;

    public Task<MemoryReviewApprovalResult> ApproveAsync(
        MemoryReviewQueueItem row,
        int userId,
        DateTimeOffset at,
        CancellationToken cancellationToken)
    {
        var payload = MemoryReviewProposalJson.ParseContradictionDetected(row.ProposedChangeJson);
        return Task.FromResult(new MemoryReviewApprovalResult(payload.SemanticMemoryId, null));
    }
}
