using Platform.Application.Abstractions.Memory.Review;
using Platform.Application.Abstractions.Memory.Semantic;
using Platform.Application.Features.Memory.Review;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Infrastructure.Features.Memory.Review.Approval;

internal sealed class ReviseSemanticClaimApprovalHandler(ISemanticMemoryService semanticService)
    : IMemoryReviewApprovalHandler
{
    public MemoryReviewProposalType ProposalType => MemoryReviewProposalType.ReviseSemanticClaim;

    public async Task<MemoryReviewApprovalResult> ApproveAsync(
        MemoryReviewQueueItem row,
        int userId,
        DateTimeOffset at,
        CancellationToken cancellationToken)
    {
        var payload = MemoryReviewProposalJson.ParseReviseSemanticClaim(row.ProposedChangeJson);
        var revised = await semanticService
            .ReviseClaimAsync(
                payload.SemanticMemoryId,
                userId,
                payload.NewClaim,
                payload.NewDomain,
                payload.NewConfidence,
                cancellationToken)
            .ConfigureAwait(false);
        return new(revised.Id, null);
    }
}
