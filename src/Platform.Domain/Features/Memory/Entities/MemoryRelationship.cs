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

    public static MemoryRelationship Define(
        int userId,
        string fromEntity,
        MemoryRelationshipType relationType,
        string toEntity,
        double strength,
        string? source,
        DateTimeOffset at)
    {
        if (string.IsNullOrWhiteSpace(fromEntity) || string.IsNullOrWhiteSpace(toEntity))
        {
            throw new MemoryDomainException("Relationship endpoints must be non-empty.");
        }

        MemoryValueConstraints.ThrowIfOutOf01(nameof(strength), strength);

        if (string.Equals(fromEntity.Trim(), toEntity.Trim(), StringComparison.Ordinal))
        {
            throw new MemoryDomainException("Self-referential relationships are not allowed.");
        }

        if (relationType is MemoryRelationshipType.Unspecified)
        {
            throw new MemoryDomainException("RelationType must be specified when creating a memory relationship.");
        }

        return new MemoryRelationship
        {
            UserId = userId,
            FromEntity = fromEntity.Trim(),
            RelationType = relationType,
            ToEntity = toEntity.Trim(),
            Strength = strength,
            Source = string.IsNullOrWhiteSpace(source) ? null : source.Trim(),
            CreatedAt = at,
        };
    }
}
