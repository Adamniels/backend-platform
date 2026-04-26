using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.ValueObjects;

namespace Platform.Domain.Features.Memory.Entities;

/// <summary>Canonical item row (see master doc <c>memory_items</c>). Not mapped to EF until migrations exist.</summary>
public sealed class MemoryItem
{
    public long Id { get; set; }
    public int PrincipalId { get; set; }
    public MemoryItemType MemoryType { get; set; }
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public string? StructuredJson { get; set; }
    public string SourceType { get; set; } = "";
    public AuthorityWeight AuthorityWeight { get; set; }
    public double Confidence { get; set; }
    public double Importance { get; set; }
    public double FreshnessScore { get; set; }
    public MemoryItemStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? LastAccessedAt { get; set; }
}
