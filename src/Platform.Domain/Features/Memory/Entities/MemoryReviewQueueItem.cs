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

    public long? ApprovedSemanticMemoryId { get; set; }
    public SemanticMemory? ApprovedSemanticMemory { get; set; }

    public long? ApprovedProceduralRuleId { get; set; }
    public ProceduralRule? ApprovedProceduralRule { get; set; }

    public string? RejectedReason { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public string? ReviewNotes { get; set; }

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

    public void Approve(
        DateTimeOffset at,
        long? approvedSemanticMemoryId,
        long? approvedProceduralRuleId,
        string? reviewNotes)
    {
        EnsurePending();
        Status = MemoryReviewStatus.Approved;
        ApprovedSemanticMemoryId = approvedSemanticMemoryId;
        ApprovedProceduralRuleId = approvedProceduralRuleId;
        ReviewNotes = string.IsNullOrWhiteSpace(reviewNotes) ? null : reviewNotes.Trim();
        ResolvedAt = at;
        UpdatedAt = at;
    }

    public void Reject(DateTimeOffset at, string? reason)
    {
        EnsurePending();
        Status = MemoryReviewStatus.Rejected;
        RejectedReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        ResolvedAt = at;
        UpdatedAt = at;
    }

    public void ApplyPendingEdits(
        string? newTitle,
        string? newSummary,
        string? newProposedChangeJson,
        DateTimeOffset at)
    {
        if (Status != MemoryReviewStatus.Pending)
        {
            throw new MemoryDomainException("Only pending review items can be edited.");
        }

        if (!string.IsNullOrWhiteSpace(newTitle))
        {
            Title = newTitle.Trim();
        }

        if (newSummary is not null)
        {
            Summary = newSummary.Trim();
        }

        if (newProposedChangeJson is not null)
        {
            ProposedChangeJson = newProposedChangeJson;
        }

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
