using Platform.Domain.Features.Memory;

namespace Platform.Domain.Features.Memory.Entities;

/// <summary>Proposed change awaiting user action (see <c>memory_review_queue</c>).</summary>
public sealed class MemoryReviewQueueItem
{
    public long Id { get; set; }
    public int PrincipalId { get; set; }
    public MemoryReviewProposalType ProposalType { get; set; }
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
    public string? ProposedChangeJson { get; set; }
    public string? EvidenceJson { get; set; }
    public int Priority { get; set; }
    public MemoryReviewStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
