namespace Platform.Application.Features.Memory.Semantic.AttachSemanticMemoryEvidence;

public sealed record AttachSemanticMemoryEvidenceCommand(
    long SemanticMemoryId,
    int UserId,
    long EventId,
    double Strength,
    string? Reason,
    bool FromInferredSource,
    bool ReinforceConfidence,
    double ReinforceConfidenceDelta,
    DateTimeOffset? EventOccurredAt,
    string? Polarity = null,
    string? SourceKind = null,
    double? ReliabilityWeight = null,
    string? SourceId = null,
    string? SchemaVersion = null,
    string? ProvenanceJson = null);
