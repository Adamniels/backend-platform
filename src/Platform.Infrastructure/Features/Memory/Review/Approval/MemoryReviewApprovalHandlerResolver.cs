using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Infrastructure.Features.Memory.Review.Approval;

internal sealed class MemoryReviewApprovalHandlerResolver(IEnumerable<IMemoryReviewApprovalHandler> handlers)
    : IMemoryReviewApprovalHandlerResolver
{
    private readonly Dictionary<MemoryReviewProposalType, IMemoryReviewApprovalHandler> _handlers = handlers
        .ToDictionary(x => x.ProposalType);

    public IMemoryReviewApprovalHandler Resolve(MemoryReviewProposalType proposalType)
    {
        if (_handlers.TryGetValue(proposalType, out var direct))
        {
            return direct;
        }

        if (proposalType == MemoryReviewProposalType.MergeDuplicate &&
            _handlers.TryGetValue(MemoryReviewProposalType.MergeSemanticCandidates, out var merge))
        {
            return merge;
        }

        throw new MemoryDomainException($"Proposal type {proposalType} is not supported for approval in v1.");
    }
}
