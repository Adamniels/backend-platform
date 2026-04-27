using Platform.Domain.Features.Memory;

namespace Platform.Application.Features.Memory.Semantic.CreateSemanticMemory;

public sealed record CreateSemanticMemoryCommand(
    int UserId,
    string Key,
    string Claim,
    double Confidence,
    double? AuthorityWeight,
    string? Domain,
    string? Status,
    long EventId,
    double EvidenceStrength,
    string? EvidenceReason,
    string? EvidencePolarity = null,
    string? EvidenceSourceKind = null,
    double? EvidenceReliabilityWeight = null,
    string? EvidenceSourceId = null,
    string? EvidenceSchemaVersion = null,
    string? EvidenceProvenanceJson = null);

public static class SemanticMemoryInitialStatus
{
    public static SemanticMemoryStatus Parse(string? status) =>
        status?.Trim()
            .ToLowerInvariant() switch
        {
            null or "" or "active" => SemanticMemoryStatus.Active,
            "pending" or "pendingreview" => SemanticMemoryStatus.PendingReview,
            _ => throw new MemoryDomainException("Status must be Active or Pending (pending review)."),
        };
}

public static class SemanticEvidenceContractParser
{
    public static MemoryEvidencePolarity ParsePolarity(string? value) =>
        value?.Trim()
            .ToLowerInvariant() switch
        {
            null or "" or "support" => MemoryEvidencePolarity.Support,
            "contradict" or "contradiction" => MemoryEvidencePolarity.Contradict,
            "weaksupport" or "weak_support" => MemoryEvidencePolarity.WeakSupport,
            "weakcontradict" or "weak_contradict" or "weakcontradiction" => MemoryEvidencePolarity.WeakContradict,
            "supersede" or "supersedes" => MemoryEvidencePolarity.Supersede,
            _ => throw new MemoryDomainException("Evidence polarity is not supported."),
        };

    public static MemoryEvidenceSourceKind ParseSourceKind(string? value) =>
        value?.Trim()
            .ToLowerInvariant() switch
        {
            null or "" or "systemheuristic" or "system_heuristic" => MemoryEvidenceSourceKind.SystemHeuristic,
            "useraction" or "user_action" => MemoryEvidenceSourceKind.UserAction,
            "workflow" => MemoryEvidenceSourceKind.Workflow,
            "importeddocument" or "imported_document" => MemoryEvidenceSourceKind.ImportedDocument,
            "llmextraction" or "llm_extraction" => MemoryEvidenceSourceKind.LlmExtraction,
            "reviewdecision" or "review_decision" => MemoryEvidenceSourceKind.ReviewDecision,
            "explicitprofile" or "explicit_profile" => MemoryEvidenceSourceKind.ExplicitProfile,
            _ => throw new MemoryDomainException("Evidence source kind is not supported."),
        };
}
