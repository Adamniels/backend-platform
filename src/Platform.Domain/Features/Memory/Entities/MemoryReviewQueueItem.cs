using Platform.Domain.Features.Memory;

namespace Platform.Domain.Features.Memory.Entities;

public sealed class MemoryReviewQueueItem
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public MemoryUser? User { get; set; }

    public MemoryReviewProposalType ProposalType { get; set; }
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
    public string? ProposedChangeJson { get; set; }
    public string? EvidenceJson { get; set; }
    public int Priority { get; set; }
    public MemoryReviewStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public static MemoryReviewQueueItem Propose(
        int userId,
        MemoryReviewProposalType proposalType,
        string title,
        string summary,
        string? proposedChangeJson,
        string? evidenceJson,
        int priority,
        DateTimeOffset at)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new MemoryDomainException("A review item requires a title.");
        }

        return new MemoryReviewQueueItem
        {
            UserId = userId,
            ProposalType = proposalType,
            Title = title.Trim(),
            Summary = (summary ?? "").Trim(),
            ProposedChangeJson = proposedChangeJson,
            EvidenceJson = evidenceJson,
            Priority = priority,
            Status = MemoryReviewStatus.Pending,
            CreatedAt = at,
            UpdatedAt = at,
        };
    }

    public void Approve(DateTimeOffset at)
    {
        EnsurePending();
        Status = MemoryReviewStatus.Approved;
        UpdatedAt = at;
    }

    public void Reject(DateTimeOffset at)
    {
        EnsurePending();
        Status = MemoryReviewStatus.Rejected;
        UpdatedAt = at;
    }

    public void MarkSuperseded(DateTimeOffset at)
    {
        if (Status == MemoryReviewStatus.Pending)
        {
            Status = MemoryReviewStatus.Superseded;
            UpdatedAt = at;
        }
    }

    private void EnsurePending()
    {
        if (Status != MemoryReviewStatus.Pending)
        {
            throw new MemoryDomainException("Only pending review items can be approved or rejected.");
        }
    }
}
