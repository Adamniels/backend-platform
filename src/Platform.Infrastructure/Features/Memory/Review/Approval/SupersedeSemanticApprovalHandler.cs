using Platform.Application.Abstractions.Memory.Review;
using Platform.Application.Abstractions.Memory.Semantic;
using Platform.Application.Features.Memory.Review;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Infrastructure.Features.Memory.Review.Approval;

internal sealed class SupersedeSemanticApprovalHandler(
    ISemanticMemoryService semanticService,
    IMemorySemanticMergeService semanticMerge) : IMemoryReviewApprovalHandler
{
    public MemoryReviewProposalType ProposalType => MemoryReviewProposalType.SupersedeSemantic;

    public async Task<MemoryReviewApprovalResult> ApproveAsync(
        MemoryReviewQueueItem row,
        int userId,
        DateTimeOffset at,
        CancellationToken cancellationToken)
    {
        var payload = MemoryReviewProposalJson.ParseSupersedeSemantic(row.ProposedChangeJson);
        var canonical = await semanticService
            .GetByIdAsync(payload.CanonicalSemanticId, userId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new MemoryDomainException("Canonical semantic was not found for this user.");
        _ = await semanticService
            .GetByIdAsync(payload.SupersededSemanticId, userId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new MemoryDomainException("Superseded semantic was not found for this user.");
        var semanticId = await semanticMerge
            .MergeApprovedAsync(
                userId,
                [payload.SupersededSemanticId, payload.CanonicalSemanticId],
                payload.CanonicalSemanticId,
                canonical.Claim,
                canonical.Domain,
                at,
                cancellationToken)
            .ConfigureAwait(false);
        return new(semanticId, null);
    }
}
