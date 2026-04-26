namespace Platform.Application.Features.Memory.Semantic.ListSemanticMemoryEvidence;

public readonly record struct ListSemanticMemoryEvidenceQuery(
    int UserId,
    long SemanticMemoryId,
    int Take);
