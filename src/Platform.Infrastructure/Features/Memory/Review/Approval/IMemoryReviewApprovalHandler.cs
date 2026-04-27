using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;
using Platform.Application.Abstractions.Memory.Review;

namespace Platform.Infrastructure.Features.Memory.Review.Approval;

public interface IMemoryReviewApprovalHandler
{
    MemoryReviewProposalType ProposalType { get; }

    Task<MemoryReviewApprovalResult> ApproveAsync(
        MemoryReviewQueueItem row,
        int userId,
        DateTimeOffset at,
        CancellationToken cancellationToken);
}

public interface IMemoryReviewApprovalHandlerResolver
{
    IMemoryReviewApprovalHandler Resolve(MemoryReviewProposalType proposalType);
}
