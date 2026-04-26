using Platform.Domain.Features.Memory;

namespace Platform.Domain.Features.Memory.Entities;

public sealed class MemoryEvidence
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public MemoryUser? User { get; set; }

    public long SemanticMemoryId { get; set; }
    public SemanticMemory? SemanticMemory { get; set; }
    public long EventId { get; set; }
    public MemoryEvent? SourceEvent { get; set; }
    public double Strength { get; set; }
    public string? Reason { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public static MemoryEvidence Link(
        int userId,
        long semanticMemoryId,
        long eventId,
        double strength,
        string? reason,
        DateTimeOffset at)
    {
        if (semanticMemoryId <= 0 || eventId <= 0)
        {
            throw new MemoryDomainException("Evidence must reference valid semantic and event ids.");
        }

        MemoryValueConstraints.ThrowIfOutOf01(nameof(strength), strength);

        return new MemoryEvidence
        {
            UserId = userId,
            SemanticMemoryId = semanticMemoryId,
            EventId = eventId,
            Strength = strength,
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            CreatedAt = at,
        };
    }
}
