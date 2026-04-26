using Platform.Domain.Features.Memory;

namespace Platform.Domain.Features.Memory.Entities;

public sealed class MemoryRelationship
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public MemoryUser? User { get; set; }

    public string FromEntity { get; set; } = "";
    public MemoryRelationshipType RelationType { get; set; }
    public string ToEntity { get; set; } = "";
    public double Strength { get; set; }
    public string? Source { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
