using Platform.Domain.Features.Memory;

namespace Platform.Domain.Features.Memory.Entities;

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
}
