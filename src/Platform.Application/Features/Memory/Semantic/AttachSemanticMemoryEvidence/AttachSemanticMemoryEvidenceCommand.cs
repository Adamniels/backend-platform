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
    DateTimeOffset? EventOccurredAt);
