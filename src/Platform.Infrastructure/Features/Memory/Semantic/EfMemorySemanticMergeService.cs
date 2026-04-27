using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions.Memory.Confidence;
using Platform.Application.Abstractions.Memory.Semantic;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;
using Platform.Domain.Features.Memory.ValueObjects;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.Memory.Semantic;

public sealed class EfMemorySemanticMergeService(
    PlatformDbContext db,
    IMemoryConfidencePolicy confidencePolicy) : IMemorySemanticMergeService
{
    public async Task<long> MergeApprovedAsync(
        int userId,
        IReadOnlyList<long> sourceSemanticIds,
        long canonicalSemanticId,
        string resultingClaim,
        string? domain,
        DateTimeOffset at,
        CancellationToken cancellationToken = default)
    {
        var ids = sourceSemanticIds
            .Append(canonicalSemanticId)
            .Where(x => x > 0)
            .Distinct()
            .ToList();
        if (ids.Count < 2)
        {
            throw new MemoryDomainException("Merge requires at least two semantic memories.");
        }

        var rows = await db.SemanticMemories
            .Where(x => x.UserId == userId && ids.Contains(x.Id))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        if (rows.Count != ids.Count)
        {
            throw new MemoryDomainException("One or more semantic merge candidates were not found for this user.");
        }

        var canonical = rows.FirstOrDefault(x => x.Id == canonicalSemanticId)
            ?? throw new MemoryDomainException("Canonical semantic was not found for this user.");
        if (string.IsNullOrWhiteSpace(resultingClaim))
        {
            throw new MemoryDomainException("Resulting claim is required for semantic merge.");
        }

        var ownsTransaction = db.Database.CurrentTransaction is null;
        await using var tx = ownsTransaction
            ? await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false)
            : null;
        try
        {
            canonical.ApplyUserApprovedRevision(
                resultingClaim,
                canonical.Confidence,
                AuthorityWeight.UserApprovedSemantic,
                at);
            if (!string.IsNullOrWhiteSpace(domain))
            {
                canonical.Domain = domain.Trim();
            }

            var movedEventIds = await db.MemoryEvidences
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.SemanticMemoryId == canonical.Id)
                .Select(x => x.EventId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            var eventIdsOnCanonical = movedEventIds.ToHashSet();
            foreach (var old in rows.Where(x => x.Id != canonical.Id))
            {
                var evidence = await db.MemoryEvidences
                    .Where(x => x.UserId == userId && x.SemanticMemoryId == old.Id)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
                foreach (var ev in evidence)
                {
                    if (eventIdsOnCanonical.Contains(ev.EventId))
                    {
                        continue;
                    }

                    ev.SemanticMemoryId = canonical.Id;
                    eventIdsOnCanonical.Add(ev.EventId);
                }

                old.MarkSuperseded(at);
                db.MemoryRelationships.Add(
                    MemoryRelationship.Define(
                        userId,
                        $"semantic:{canonical.Id}",
                        MemoryRelationshipType.Supersedes,
                        $"semantic:{old.Id}",
                        1d,
                        "review.merge",
                        at,
                        "semantic",
                        "semantic"));
            }

            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await RecomputeCanonicalAsync(userId, canonical, at, cancellationToken).ConfigureAwait(false);
            if (ownsTransaction)
            {
                await tx!.CommitAsync(cancellationToken).ConfigureAwait(false);
            }

            return canonical.Id;
        }
        catch
        {
            if (ownsTransaction && tx is not null)
            {
                await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
            }

            throw;
        }
    }

    private async Task RecomputeCanonicalAsync(
        int userId,
        SemanticMemory canonical,
        DateTimeOffset at,
        CancellationToken cancellationToken)
    {
        var signals = await (
                from e in db.MemoryEvidences.AsNoTracking()
                join ev in db.MemoryEvents.AsNoTracking() on e.EventId equals ev.Id
                where e.UserId == userId && e.SemanticMemoryId == canonical.Id && ev.UserId == userId
                select new SemanticEvidenceSignal(
                    e.Polarity,
                    e.SourceKind,
                    e.Strength,
                    e.ReliabilityWeight,
                    ev.OccurredAt))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var computed = confidencePolicy.Compute(canonical, signals, conflictsWithExplicitProfile: false, at);
        canonical.SetConfidence(computed.Confidence, at);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
