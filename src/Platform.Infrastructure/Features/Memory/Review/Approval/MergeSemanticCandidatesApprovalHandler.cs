using Platform.Application.Abstractions.Memory.Review;
using Platform.Application.Abstractions.Memory.Semantic;
using Platform.Application.Features.Memory.Review;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Infrastructure.Features.Memory.Review.Approval;

internal sealed class MergeSemanticCandidatesApprovalHandler(IMemorySemanticMergeService semanticMerge)
    : IMemoryReviewApprovalHandler
{
    public MemoryReviewProposalType ProposalType => MemoryReviewProposalType.MergeSemanticCandidates;

    public async Task<MemoryReviewApprovalResult> ApproveAsync(
        MemoryReviewQueueItem row,
        int userId,
        DateTimeOffset at,
        CancellationToken cancellationToken)
    {
        var payload = MemoryReviewProposalJson.ParseMergeSemanticCandidates(row.ProposedChangeJson);
        var semanticId = await semanticMerge
            .MergeApprovedAsync(
                userId,
                payload.SourceSemanticIds,
                payload.CanonicalSemanticId,
                payload.ResultingClaim,
                payload.Domain,
                at,
                cancellationToken)
            .ConfigureAwait(false);
        return new(semanticId, null);
    }
}
