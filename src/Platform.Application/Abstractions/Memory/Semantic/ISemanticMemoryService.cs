using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Abstractions.Memory.Semantic;

/// <summary>Learned claims with confidence and evidence links; no vector or LLM in this port.</summary>
public interface ISemanticMemoryService
{
    Task<IReadOnlyList<SemanticMemory>> ListForUserAsync(
        int userId,
        bool includePendingReview = true,
        CancellationToken cancellationToken = default);

    Task<SemanticMemory?> GetByIdAsync(
        long id,
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>Requires at least one <see cref="MemoryEvidence" /> row (initial link on create).</summary>
    Task<SemanticMemory> CreateWithInitialEvidenceAsync(
        int userId,
        string key,
        string claim,
        double confidence,
        double authorityWeight,
        string? domain,
        SemanticMemoryStatus initialStatus,
        long eventId,
        double evidenceStrength,
        string? evidenceReason,
        CancellationToken cancellationToken = default);

    Task<SemanticMemory> SetConfidenceAsync(
        long id,
        int userId,
        double newConfidence,
        bool fromInferredSource,
        CancellationToken cancellationToken = default);

    /// <param name="fromInferredSource">If true, applies the inferred-override floor (user-approved semantics stay immutable).</param>
    Task<SemanticMemory> AttachEvidenceAsync(
        long id,
        int userId,
        long eventId,
        double strength,
        string? reason,
        bool fromInferredSource,
        bool reinforce,
        double reinforceConfidenceDelta,
        DateTimeOffset? eventOccurredAtForReinforce,
        CancellationToken cancellationToken = default);

    Task<SemanticMemory> ArchiveAsync(
        long id,
        int userId,
        CancellationToken cancellationToken = default);

    Task<SemanticMemory> RejectAsync(
        long id,
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>Case-insensitive key and normalized domain; only Active or PendingReview.</summary>
    Task<SemanticMemory?> FindActiveOrPendingByKeyDomainAsync(
        int userId,
        string key,
        string? domain,
        CancellationToken cancellationToken = default);

    /// <summary>Prefix “similarity” (no vectors): key substring match and/or exact domain, Active+Pending only.</summary>
    Task<IReadOnlyList<SemanticMemory>> FindSimilarByKeyOrDomainAsync(
        int userId,
        string? keySubstring,
        string? domain,
        int take,
        CancellationToken cancellationToken = default);
}
