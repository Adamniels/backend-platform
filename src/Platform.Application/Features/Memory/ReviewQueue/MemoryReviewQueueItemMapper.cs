using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Features.Memory.ReviewQueue;

public static class MemoryReviewQueueItemMapper
{
    public static MemoryReviewQueueItemV1Dto ToV1Dto(MemoryReviewQueueItem x) =>
        new(
            x.Id,
            x.Title,
            x.Summary,
            x.Status.ToString(),
            x.ProposalType.ToString(),
            x.Priority,
            x.CreatedAt.ToString("O"),
            x.UpdatedAt.ToString("O"),
            x.ApprovedSemanticMemoryId,
            x.RejectedReason,
            x.ResolvedAt?.ToString("O"),
            x.ReviewNotes,
            x.ProposedChangeJson,
            x.EvidenceJson,
            x.ApprovedProceduralRuleId);

    public static IReadOnlyList<MemoryReviewQueueItemV1Dto> ToV1Dtos(IReadOnlyList<MemoryReviewQueueItem> items) =>
        items.Select(ToV1Dto).ToList();
}
