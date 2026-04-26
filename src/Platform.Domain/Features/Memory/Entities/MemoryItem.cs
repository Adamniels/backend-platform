using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.ValueObjects;

namespace Platform.Domain.Features.Memory.Entities;

/// <summary>Canonical memory row: profile facts, notes, inferred items, documents (see master doc).</summary>
public sealed class MemoryItem
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public MemoryUser? User { get; set; }

    public MemoryItemType MemoryType { get; set; }
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public string? StructuredJson { get; set; }
    public string SourceType { get; set; } = "";
    public double AuthorityWeight { get; set; }
    public double Confidence { get; set; }
    public double Importance { get; set; }
    public double FreshnessScore { get; set; }
    public MemoryItemStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? LastAccessedAt { get; set; }

    public static MemoryItem CreateNew(
        int userId,
        MemoryItemType memoryType,
        string title,
        string content,
        string sourceType,
        double authorityWeight,
        double confidence,
        DateTimeOffset at)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new MemoryDomainException("MemoryItem title is required.");
        }

        MemoryValueConstraints.ThrowIfOutOf01(nameof(authorityWeight), authorityWeight);
        MemoryValueConstraints.ThrowIfOutOf01(nameof(confidence), confidence);
        var importance = MemoryValueConstraints.Clamp01(0.5);
        var freshness = MemoryValueConstraints.Clamp01(0.5);

        return new MemoryItem
        {
            UserId = userId,
            MemoryType = memoryType,
            Title = title.Trim(),
            Content = content ?? "",
            SourceType = sourceType ?? "",
            AuthorityWeight = authorityWeight,
            Confidence = confidence,
            Importance = importance,
            FreshnessScore = freshness,
            Status = MemoryItemStatus.Draft,
            CreatedAt = at,
            UpdatedAt = at,
        };
    }

    public void RecordAccess(DateTimeOffset at)
    {
        if (Status is MemoryItemStatus.Archived or MemoryItemStatus.Superseded)
        {
            return;
        }

        LastAccessedAt = at;
        UpdatedAt = at;
    }

    public void Archive(DateTimeOffset at)
    {
        if (Status is MemoryItemStatus.Superseded)
        {
            throw new MemoryDomainException("Cannot archive a superseded memory item.");
        }

        Status = MemoryItemStatus.Archived;
        UpdatedAt = at;
    }

    public void MarkSuperseded(DateTimeOffset at)
    {
        Status = MemoryItemStatus.Superseded;
        UpdatedAt = at;
    }

    public void PromoteToActive(DateTimeOffset at)
    {
        if (Status is not MemoryItemStatus.Draft)
        {
            throw new MemoryDomainException("Only draft items can be promoted to active.");
        }

        Status = MemoryItemStatus.Active;
        UpdatedAt = at;
    }

    public void ApplyScoredUpdate(
        double importance,
        double freshnessScore,
        double confidence,
        DateTimeOffset at)
    {
        MemoryValueConstraints.ThrowIfOutOf01(nameof(confidence), confidence);
        Importance = MemoryValueConstraints.Clamp01(importance);
        FreshnessScore = MemoryValueConstraints.Clamp01(freshnessScore);
        Confidence = confidence;
        UpdatedAt = at;
    }

    public void SetAuthority(AuthorityWeight weight, DateTimeOffset at)
    {
        weight.ThrowIfNotValid();
        AuthorityWeight = weight.Value;
        UpdatedAt = at;
    }

    public bool IsProfileFact() => MemoryType is MemoryItemType.ProfileFact;
}
