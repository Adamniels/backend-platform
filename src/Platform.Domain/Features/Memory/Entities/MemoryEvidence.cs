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
    public MemoryEvidencePolarity Polarity { get; set; } = MemoryEvidencePolarity.Support;
    public MemoryEvidenceSourceKind SourceKind { get; set; } = MemoryEvidenceSourceKind.SystemHeuristic;
    public double ReliabilityWeight { get; set; } = 0.55d;
    public string? SourceId { get; set; }
    public string? SchemaVersion { get; set; }
    public string? ProvenanceJson { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public static MemoryEvidence Link(
        int userId,
        long semanticMemoryId,
        long eventId,
        double strength,
        string? reason,
        DateTimeOffset at,
        MemoryEvidencePolarity polarity = MemoryEvidencePolarity.Support,
        MemoryEvidenceSourceKind sourceKind = MemoryEvidenceSourceKind.SystemHeuristic,
        double reliabilityWeight = 0.55d,
        string? sourceId = null,
        string? schemaVersion = null,
        string? provenanceJson = null)
    {
        if (semanticMemoryId <= 0 || eventId <= 0)
        {
            throw new MemoryDomainException("Evidence must reference valid semantic and event ids.");
        }

        MemoryValueConstraints.ThrowIfOutOf01(nameof(strength), strength);
        MemoryValueConstraints.ThrowIfOutOf01(nameof(reliabilityWeight), reliabilityWeight);
        if (polarity is 0)
        {
            throw new MemoryDomainException("Evidence polarity must be specified.");
        }

        if (sourceKind is MemoryEvidenceSourceKind.Unspecified)
        {
            throw new MemoryDomainException("Evidence source kind must be specified.");
        }

        return new MemoryEvidence
        {
            UserId = userId,
            SemanticMemoryId = semanticMemoryId,
            EventId = eventId,
            Strength = strength,
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            Polarity = polarity,
            SourceKind = sourceKind,
            ReliabilityWeight = reliabilityWeight,
            SourceId = string.IsNullOrWhiteSpace(sourceId) ? null : sourceId.Trim(),
            SchemaVersion = string.IsNullOrWhiteSpace(schemaVersion) ? null : schemaVersion.Trim(),
            ProvenanceJson = string.IsNullOrWhiteSpace(provenanceJson) ? null : provenanceJson,
            CreatedAt = at,
        };
    }
}
