using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions.Memory.Contradictions;
using Platform.Application.Abstractions.Memory.Maintenance;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.Memory.Maintenance;

public sealed class EfSemanticMemoryMaintenanceService(
    PlatformDbContext db,
    IExplicitProfileConflictDetector explicitConflicts,
    ISemanticConfidenceRecomputeService confidenceRecompute,
    IStaleSemanticPolicy stalePolicy,
    IContradictionEvaluationService contradictionEvaluation,
    ISemanticDuplicateDetector duplicateDetector,
    IMemoryReviewProposalEmitter reviewProposalEmitter) : ISemanticMemoryMaintenanceService
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
            var computed = await confidenceRecompute
                .ComputeAsync(userId, s, conflictIds.Contains(s.Id), now, cancellationToken)
                .ConfigureAwait(false);
            if (Math.Abs(s.Confidence - computed.Confidence) > 0.001d)
            {
                s.SetConfidence(computed.Confidence, now);
                recomputed++;
            }

            if (contradictionEvaluation.ShouldCreateContradictionProposal(s, computed))
            {
                if (await reviewProposalEmitter
                        .TryEmitContradictionAsync(userId, s, computed, now, cancellationToken)
                        .ConfigureAwait(false))
                {
                    contradictionProposals++;
                }
            }

            if (stalePolicy.ShouldCreateArchiveProposal(s, now, computed))
            {
                if (await reviewProposalEmitter
                        .TryEmitStaleAsync(userId, s, now, cancellationToken)
                        .ConfigureAwait(false))
                {
                    staleProposals++;
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var mergeProposals = await reviewProposalEmitter
            .EmitMergeCandidatesAsync(userId, duplicateDetector.Detect(semantics), now, cancellationToken)
            .ConfigureAwait(false);
        return new(recomputed, staleProposals, contradictionProposals, mergeProposals);
    }
}
