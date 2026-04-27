using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions.Memory.Confidence;
using Platform.Application.Abstractions.Memory.Review;
using Platform.Application.Features.Memory.Review;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.Memory.Maintenance;

public interface ISemanticConfidenceRecomputeService
{
    Task<SemanticConfidenceComputation> ComputeAsync(
        int userId,
        SemanticMemory semanticMemory,
        bool conflictsWithExplicitProfile,
        DateTimeOffset now,
        CancellationToken cancellationToken = default);
}

public interface IStaleSemanticPolicy
{
    bool ShouldCreateArchiveProposal(
        SemanticMemory semanticMemory,
        DateTimeOffset now,
        SemanticConfidenceComputation computed);
}

public interface IContradictionEvaluationService
{
    bool ShouldCreateContradictionProposal(
        SemanticMemory semanticMemory,
        SemanticConfidenceComputation computed);
}

public sealed record SemanticDuplicateGroup(
    SemanticMemory Canonical,
    IReadOnlyList<long> SourceSemanticIds);

public interface ISemanticDuplicateDetector
{
    IReadOnlyList<SemanticDuplicateGroup> Detect(IReadOnlyList<SemanticMemory> semantics);
}

public interface IMemoryReviewProposalEmitter
{
    Task<bool> TryEmitContradictionAsync(
        int userId,
        SemanticMemory semanticMemory,
        SemanticConfidenceComputation computed,
        DateTimeOffset now,
        CancellationToken cancellationToken = default);

    Task<bool> TryEmitStaleAsync(
        int userId,
        SemanticMemory semanticMemory,
        DateTimeOffset now,
        CancellationToken cancellationToken = default);

    Task<int> EmitMergeCandidatesAsync(
        int userId,
        IReadOnlyList<SemanticDuplicateGroup> duplicates,
        DateTimeOffset now,
        CancellationToken cancellationToken = default);
}

public sealed class EfSemanticConfidenceRecomputeService(
    PlatformDbContext db,
    IMemoryConfidencePolicy confidencePolicy) : ISemanticConfidenceRecomputeService
{
    public async Task<SemanticConfidenceComputation> ComputeAsync(
        int userId,
        SemanticMemory semanticMemory,
        bool conflictsWithExplicitProfile,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        var signals = await (
                from e in db.MemoryEvidences.AsNoTracking()
                join ev in db.MemoryEvents.AsNoTracking() on e.EventId equals ev.Id
                where e.UserId == userId && e.SemanticMemoryId == semanticMemory.Id && ev.UserId == userId
                select new SemanticEvidenceSignal(
                    e.Polarity,
                    e.SourceKind,
                    e.Strength,
                    e.ReliabilityWeight,
                    ev.OccurredAt))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return confidencePolicy.Compute(semanticMemory, signals, conflictsWithExplicitProfile, now);
    }
}

public sealed class DefaultStaleSemanticPolicy : IStaleSemanticPolicy
{
    public bool ShouldCreateArchiveProposal(
        SemanticMemory semanticMemory,
        DateTimeOffset now,
        SemanticConfidenceComputation computed)
    {
        if (semanticMemory.AuthorityWeight >= Platform.Domain.Features.Memory.ValueObjects.AuthorityWeight.UserApprovedSemantic.Value)
        {
            return false;
        }

        var ageDays = semanticMemory.LastSupportedAt is null
            ? (now - semanticMemory.UpdatedAt).TotalDays
            : (now - semanticMemory.LastSupportedAt.Value).TotalDays;
        return ageDays >= 90d && computed.Confidence < 0.42d;
    }
}

public sealed class DefaultContradictionEvaluationService : IContradictionEvaluationService
{
    public bool ShouldCreateContradictionProposal(
        SemanticMemory semanticMemory,
        SemanticConfidenceComputation computed) =>
        computed.HasMaterialContradiction &&
        semanticMemory.AuthorityWeight < Platform.Domain.Features.Memory.ValueObjects.AuthorityWeight.UserApprovedSemantic.Value;
}

public sealed class DefaultSemanticDuplicateDetector : ISemanticDuplicateDetector
{
    public IReadOnlyList<SemanticDuplicateGroup> Detect(IReadOnlyList<SemanticMemory> semantics) =>
        semantics
            .Where(x => x.Status is SemanticMemoryStatus.Active or SemanticMemoryStatus.PendingReview)
            .GroupBy(x => $"{x.Key.Trim().ToLowerInvariant()}::{x.Domain?.Trim().ToLowerInvariant() ?? ""}")
            .Where(g => g.Select(x => x.Claim.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1)
            .Select(
                g =>
                {
                    var rows = g.OrderByDescending(x => x.AuthorityWeight)
                        .ThenByDescending(x => x.Confidence)
                        .ToList();
                    return new SemanticDuplicateGroup(rows[0], rows.Select(x => x.Id).ToList());
                })
            .ToList();
}

public sealed class MemoryReviewProposalEmitter(IMemoryReviewService reviews) : IMemoryReviewProposalEmitter
{
    public async Task<bool> TryEmitContradictionAsync(
        int userId,
        SemanticMemory semanticMemory,
        SemanticConfidenceComputation computed,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        var fingerprint = $"contradiction:{userId}:{semanticMemory.Id}:{semanticMemory.UpdatedAt:yyyyMMddHHmmss}";
        if (await reviews
                .HasPendingWithFingerprintAsync(
                    userId,
                    MemoryReviewProposalType.ContradictionDetected,
                    fingerprint,
                    cancellationToken)
                .ConfigureAwait(false))
        {
            return false;
        }

        var proposal = new ContradictionDetectedProposalV1
        {
            SemanticMemoryId = semanticMemory.Id,
            Key = semanticMemory.Key,
            Claim = semanticMemory.Claim,
            Confidence = computed.Confidence,
            SupportScore = computed.SupportScore,
            ContradictionScore = computed.ContradictionScore,
        };
        await reviews.CreatePendingAsync(
                MemoryReviewQueueItem.Propose(
                    userId,
                    MemoryReviewProposalType.ContradictionDetected,
                    $"Contradiction: {semanticMemory.Key}",
                    "Recent evidence contradicts this learned memory; review before changing its lifecycle.",
                    MemoryReviewProposalJson.SerializeContradictionDetected(proposal),
                    JsonSerializer.Serialize(new { fingerprint, source = "semantic_maintenance_v1" }),
                    dedupFingerprint: fingerprint,
                    priority: 3,
                    now),
                cancellationToken)
            .ConfigureAwait(false);
        return true;
    }

    public async Task<bool> TryEmitStaleAsync(
        int userId,
        SemanticMemory semanticMemory,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        var fingerprint = $"stale:{userId}:{semanticMemory.Id}:{semanticMemory.UpdatedAt:yyyyMMddHHmmss}";
        if (await reviews
                .HasPendingWithFingerprintAsync(
                    userId,
                    MemoryReviewProposalType.ArchiveStaleSemantic,
                    fingerprint,
                    cancellationToken)
                .ConfigureAwait(false))
        {
            return false;
        }

        var proposal = new ArchiveStaleSemanticProposalV1
        {
            SemanticMemoryId = semanticMemory.Id,
            Key = semanticMemory.Key,
            Claim = semanticMemory.Claim,
            CurrentConfidence = semanticMemory.Confidence,
            LastSupportedAt = semanticMemory.LastSupportedAt,
        };
        await reviews.CreatePendingAsync(
                MemoryReviewQueueItem.Propose(
                    userId,
                    MemoryReviewProposalType.ArchiveStaleSemantic,
                    $"Stale memory: {semanticMemory.Key}",
                    "This low-authority learned memory has little recent support and should be reviewed.",
                    MemoryReviewProposalJson.SerializeArchiveStaleSemantic(proposal),
                    JsonSerializer.Serialize(new { fingerprint, source = "semantic_maintenance_v1" }),
                    dedupFingerprint: fingerprint,
                    priority: 1,
                    now),
                cancellationToken)
            .ConfigureAwait(false);
        return true;
    }

    public async Task<int> EmitMergeCandidatesAsync(
        int userId,
        IReadOnlyList<SemanticDuplicateGroup> duplicates,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        var created = 0;
        foreach (var duplicate in duplicates)
        {
            var sourceIds = duplicate.SourceSemanticIds;
            var fingerprint = $"merge:{userId}:{string.Join("-", sourceIds.Order())}";
            if (await reviews
                    .HasPendingWithFingerprintAsync(
                        userId,
                        MemoryReviewProposalType.MergeSemanticCandidates,
                        fingerprint,
                        cancellationToken)
                    .ConfigureAwait(false))
            {
                continue;
            }

            var proposal = new MergeSemanticCandidatesProposalV1
            {
                SourceSemanticIds = sourceIds,
                CanonicalSemanticId = duplicate.Canonical.Id,
                ResultingClaim = duplicate.Canonical.Claim,
                Domain = duplicate.Canonical.Domain,
            };
            await reviews.CreatePendingAsync(
                    MemoryReviewQueueItem.Propose(
                        userId,
                        MemoryReviewProposalType.MergeSemanticCandidates,
                        $"Merge memories: {duplicate.Canonical.Key}",
                        "Multiple learned memories share a key/domain but disagree on the claim.",
                        MemoryReviewProposalJson.SerializeMergeSemanticCandidates(proposal),
                        JsonSerializer.Serialize(new { fingerprint, source = "semantic_maintenance_v1" }),
                        dedupFingerprint: fingerprint,
                        priority: 2,
                        now),
                    cancellationToken)
                .ConfigureAwait(false);
            created++;
        }

        return created;
    }
}
