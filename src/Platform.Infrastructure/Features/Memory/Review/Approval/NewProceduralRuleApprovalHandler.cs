using Platform.Application.Abstractions.Memory.Procedural;
using Platform.Application.Abstractions.Memory.Review;
using Platform.Application.Features.Memory.Review;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Infrastructure.Features.Memory.Review.Approval;

internal sealed class NewProceduralRuleApprovalHandler(IProceduralRuleService proceduralRules) : IMemoryReviewApprovalHandler
{
    public MemoryReviewProposalType ProposalType => MemoryReviewProposalType.NewProceduralRule;

    public async Task<MemoryReviewApprovalResult> ApproveAsync(
        MemoryReviewQueueItem row,
        int userId,
        DateTimeOffset at,
        CancellationToken cancellationToken)
    {
        var payload = MemoryReviewProposalJson.ParseNewProceduralRule(row.ProposedChangeJson);
        var proceduralRuleId = await proceduralRules
            .ApplyApprovedNewProceduralProposalAsync(userId, payload, at, cancellationToken)
            .ConfigureAwait(false);
        return new(null, proceduralRuleId);
    }
}
