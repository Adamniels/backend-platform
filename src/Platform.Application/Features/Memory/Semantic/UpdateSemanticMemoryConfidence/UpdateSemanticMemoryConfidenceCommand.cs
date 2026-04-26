namespace Platform.Application.Features.Memory.Semantic.UpdateSemanticMemoryConfidence;

public sealed record UpdateSemanticMemoryConfidenceCommand(
    long SemanticMemoryId,
    int UserId,
    double Confidence,
    bool FromInferredSource);
