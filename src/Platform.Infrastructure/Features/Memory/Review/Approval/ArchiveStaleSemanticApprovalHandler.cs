using Platform.Application.Abstractions.Memory.Review;
using Platform.Application.Abstractions.Memory.Semantic;
using Platform.Application.Features.Memory.Review;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Infrastructure.Features.Memory.Review.Approval;

internal sealed class ArchiveStaleSemanticApprovalHandler(ISemanticMemoryService semanticService) : IMemoryReviewApprovalHandler
{
    public MemoryReviewProposalType ProposalType => MemoryReviewProposalType.ArchiveStaleSemantic;

    public async Task<MemoryReviewApprovalResult> ApproveAsync(
        MemoryReviewQueueItem row,
        int userId,
        DateTimeOffset at,
        CancellationToken cancellationToken)
    {
        var payload = MemoryReviewProposalJson.ParseArchiveStaleSemantic(row.ProposedChangeJson);
        var archived = await semanticService
            .ArchiveAsync(payload.SemanticMemoryId, userId, cancellationToken)
            .ConfigureAwait(false);
        return new(archived.Id, null);
    }
}
