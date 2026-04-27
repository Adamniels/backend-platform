using Platform.Application.Abstractions.Memory.Procedural;
using Platform.Application.Abstractions.Memory.Review;
using Platform.Application.Features.Memory.Review;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Infrastructure.Features.Memory.Review.Approval;

internal sealed class ReviseProceduralRuleApprovalHandler(IProceduralRuleService proceduralRules)
    : IMemoryReviewApprovalHandler
{
    public MemoryReviewProposalType ProposalType => MemoryReviewProposalType.ReviseProceduralRule;

    public async Task<MemoryReviewApprovalResult> ApproveAsync(
        MemoryReviewQueueItem row,
        int userId,
        DateTimeOffset at,
        CancellationToken cancellationToken)
    {
        var payload = MemoryReviewProposalJson.ParseReviseProceduralRule(row.ProposedChangeJson);
        var proceduralRuleId = await proceduralRules
            .ApplyApprovedNewProceduralProposalAsync(
                userId,
                new NewProceduralRuleMemoryProposalV1
                {
                    BasisRuleId = payload.BasisRuleId,
                    RuleContent = payload.RuleContent,
                    Source = payload.Source,
                },
                at,
                cancellationToken)
            .ConfigureAwait(false);
        return new(null, proceduralRuleId);
    }
}
