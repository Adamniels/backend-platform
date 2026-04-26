using Platform.Domain.Features.Memory;

namespace Platform.Domain.Features.Memory.Entities;

/// <summary>Entity–relation–entity edge metadata (see <c>memory_relationships</c>).</summary>
public sealed class MemoryRelationship
{
    public long Id { get; set; }
    public string FromEntity { get; set; } = "";
    public MemoryRelationshipType RelationType { get; set; }
    public string ToEntity { get; set; } = "";
    public double Strength { get; set; }
    public string? Source { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
