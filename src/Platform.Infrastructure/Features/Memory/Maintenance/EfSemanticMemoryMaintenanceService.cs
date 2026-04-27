using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions.Memory.Confidence;
using Platform.Application.Abstractions.Memory.Contradictions;
using Platform.Application.Abstractions.Memory.Maintenance;
using Platform.Application.Abstractions.Memory.Review;
using Platform.Application.Features.Memory.Review;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;
using Platform.Domain.Features.Memory.ValueObjects;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.Memory.Maintenance;

public sealed class EfSemanticMemoryMaintenanceService(
    PlatformDbContext db,
    IMemoryConfidencePolicy confidencePolicy,
    IExplicitProfileConflictDetector explicitConflicts,
    IMemoryReviewService reviews) : ISemanticMemoryMaintenanceService
{
    public async Task<SemanticMemoryMaintenanceOutcome> RunAsync(
        int userId,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        var semantics = await db.SemanticMemories
            .Where(
                x => x.UserId == userId &&
                    (x.Status == SemanticMemoryStatus.Active || x.Status == SemanticMemoryStatus.PendingReview))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        if (semantics.Count == 0)
        {
            return new(0, 0, 0, 0);
        }

        var profile = await db.ExplicitUserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken)
            .ConfigureAwait(false);
        var conflictIds = explicitConflicts
            .Detect(profile, semantics)
            .Select(x => x.SemanticMemoryId)
            .ToHashSet();

        var recomputed = 0;
        var staleProposals = 0;
        var contradictionProposals = 0;
        foreach (var s in semantics)
        {
            var signals = await SignalsForAsync(userId, s.Id, cancellationToken).ConfigureAwait(false);
            var computed = confidencePolicy.Compute(s, signals, conflictIds.Contains(s.Id), now);
            if (Math.Abs(s.Confidence - computed.Confidence) > 0.001d)
            {
                s.SetConfidence(computed.Confidence, now);
                recomputed++;
            }

            if (computed.HasMaterialContradiction && s.AuthorityWeight < AuthorityWeight.UserApprovedSemantic.Value)
            {
                var fingerprint = $"contradiction:{userId}:{s.Id}:{s.UpdatedAt:yyyyMMddHHmmss}";
                if (!await reviews.HasPendingWithEvidenceSubstringAsync(userId, fingerprint, cancellationToken)
                        .ConfigureAwait(false))
                {
                    var proposal = new ContradictionDetectedProposalV1
                    {
                        SemanticMemoryId = s.Id,
                        Key = s.Key,
                        Claim = s.Claim,
                        Confidence = computed.Confidence,
                        SupportScore = computed.SupportScore,
                        ContradictionScore = computed.ContradictionScore,
                    };
                    await reviews.CreatePendingAsync(
                            MemoryReviewQueueItem.Propose(
                                userId,
                                MemoryReviewProposalType.ContradictionDetected,
                                $"Contradiction: {s.Key}",
                                "Recent evidence contradicts this learned memory; review before changing its lifecycle.",
                                MemoryReviewProposalJson.SerializeContradictionDetected(proposal),
                                JsonSerializer.Serialize(new { fingerprint, source = "semantic_maintenance_v1" }),
                                priority: 3,
                                now),
                            cancellationToken)
                        .ConfigureAwait(false);
                    contradictionProposals++;
                }
            }

            if (IsStaleLowAuthority(s, now, computed))
            {
                var fingerprint = $"stale:{userId}:{s.Id}:{s.UpdatedAt:yyyyMMddHHmmss}";
                if (!await reviews.HasPendingWithEvidenceSubstringAsync(userId, fingerprint, cancellationToken)
                        .ConfigureAwait(false))
                {
                    var proposal = new ArchiveStaleSemanticProposalV1
                    {
                        SemanticMemoryId = s.Id,
                        Key = s.Key,
                        Claim = s.Claim,
                        CurrentConfidence = s.Confidence,
                        LastSupportedAt = s.LastSupportedAt,
                    };
                    await reviews.CreatePendingAsync(
                            MemoryReviewQueueItem.Propose(
                                userId,
                                MemoryReviewProposalType.ArchiveStaleSemantic,
                                $"Stale memory: {s.Key}",
                                "This low-authority learned memory has little recent support and should be reviewed.",
                                MemoryReviewProposalJson.SerializeArchiveStaleSemantic(proposal),
                                JsonSerializer.Serialize(new { fingerprint, source = "semantic_maintenance_v1" }),
                                priority: 1,
                                now),
                            cancellationToken)
                        .ConfigureAwait(false);
                    staleProposals++;
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var mergeProposals = await CreateMergeProposalsAsync(userId, semantics, now, cancellationToken)
            .ConfigureAwait(false);
        return new(recomputed, staleProposals, contradictionProposals, mergeProposals);
    }

    private async Task<IReadOnlyList<SemanticEvidenceSignal>> SignalsForAsync(
        int userId,
        long semanticMemoryId,
        CancellationToken cancellationToken) =>
        await (
                from e in db.MemoryEvidences.AsNoTracking()
                join ev in db.MemoryEvents.AsNoTracking() on e.EventId equals ev.Id
                where e.UserId == userId && e.SemanticMemoryId == semanticMemoryId && ev.UserId == userId
                select new SemanticEvidenceSignal(
                    e.Polarity,
                    e.SourceKind,
                    e.Strength,
                    e.ReliabilityWeight,
                    ev.OccurredAt))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    private static bool IsStaleLowAuthority(
        SemanticMemory s,
        DateTimeOffset now,
        SemanticConfidenceComputation computed)
    {
        if (s.AuthorityWeight >= AuthorityWeight.UserApprovedSemantic.Value)
        {
            return false;
        }

        var ageDays = s.LastSupportedAt is null
            ? (now - s.UpdatedAt).TotalDays
            : (now - s.LastSupportedAt.Value).TotalDays;
        return ageDays >= 90d && computed.Confidence < 0.42d;
    }

    private async Task<int> CreateMergeProposalsAsync(
        int userId,
        IReadOnlyList<SemanticMemory> semantics,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var created = 0;
        foreach (var group in semantics
                     .Where(x => x.Status is SemanticMemoryStatus.Active or SemanticMemoryStatus.PendingReview)
                     .GroupBy(x => $"{x.Key.Trim().ToLowerInvariant()}::{x.Domain?.Trim().ToLowerInvariant() ?? ""}")
                     .Where(g => g.Select(x => x.Claim.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1))
        {
            var rows = group.OrderByDescending(x => x.AuthorityWeight)
                .ThenByDescending(x => x.Confidence)
                .ToList();
            var canonical = rows[0];
            var sourceIds = rows.Select(x => x.Id).ToList();
            var fingerprint = $"merge:{userId}:{string.Join("-", sourceIds.Order())}";
            if (await reviews.HasPendingWithEvidenceSubstringAsync(userId, fingerprint, cancellationToken)
                    .ConfigureAwait(false))
            {
                continue;
            }

            var proposal = new MergeSemanticCandidatesProposalV1
            {
                SourceSemanticIds = sourceIds,
                CanonicalSemanticId = canonical.Id,
                ResultingClaim = canonical.Claim,
                Domain = canonical.Domain,
            };
            await reviews.CreatePendingAsync(
                    MemoryReviewQueueItem.Propose(
                        userId,
                        MemoryReviewProposalType.MergeSemanticCandidates,
                        $"Merge memories: {canonical.Key}",
                        "Multiple learned memories share a key/domain but disagree on the claim.",
                        MemoryReviewProposalJson.SerializeMergeSemanticCandidates(proposal),
                        JsonSerializer.Serialize(new { fingerprint, source = "semantic_maintenance_v1" }),
                        priority: 2,
                        now),
                    cancellationToken)
                .ConfigureAwait(false);
            created++;
        }

        return created;
    }
}
