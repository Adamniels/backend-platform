using Microsoft.EntityFrameworkCore;
using Npgsql;
using Platform.Application.Abstractions.Memory.Semantic;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;
using Platform.Domain.Features.Memory.ValueObjects;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.Memory.Semantic;

public sealed class EfSemanticMemoryService(PlatformDbContext db) : ISemanticMemoryService
{
    public async Task<SemanticMemory> ArchiveAsync(
        long id,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var row = await LoadMutableAsync(id, userId, cancellationToken).ConfigureAwait(false);
        var at = DateTimeOffset.UtcNow;
        row.MarkArchived(at);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return row;
    }

    public async Task<SemanticMemory> AttachEvidenceAsync(
        long id,
        int userId,
        long eventId,
        double strength,
        string? reason,
        bool fromInferredSource,
        bool reinforce,
        double reinforceConfidenceDelta,
        DateTimeOffset? eventOccurredAtForReinforce,
        CancellationToken cancellationToken = default)
    {
        var row = await LoadMutableAsync(id, userId, cancellationToken).ConfigureAwait(false);
        await AssertEventOwnedByUserAsync(userId, eventId, cancellationToken).ConfigureAwait(false);
        var duplicateLink = await db.MemoryEvidences
            .AnyAsync(
                x => x.SemanticMemoryId == id && x.EventId == eventId,
                cancellationToken)
            .ConfigureAwait(false);
        if (duplicateLink)
        {
            return row;
        }

        var at = DateTimeOffset.UtcNow;
        var ev = MemoryEvidence.Link(userId, id, eventId, strength, reason, at);
        db.MemoryEvidences.Add(ev);
        if (reinforce)
        {
            var supported = eventOccurredAtForReinforce ?? at;
            row.ReinforceWithEvidence(
                reinforceConfidenceDelta,
                supported,
                at,
                fromInferredSource: fromInferredSource);
        }

        try
        {
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateException)
        {
            throw new MemoryDomainException("Could not attach evidence (duplicate link or invalid reference).");
        }

        return row;
    }

    public async Task<SemanticMemory> CreateWithInitialEvidenceAsync(
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
        CancellationToken cancellationToken = default)
    {
        if (!AuthorityWeight.TryCreate(authorityWeight, out var auth))
        {
            throw new MemoryDomainException("Authority weight is out of range.");
        }

        var existing = await FindActiveOrPendingByKeyDomainAsync(
                userId,
                key,
                domain,
                cancellationToken)
            .ConfigureAwait(false);
        if (existing is not null)
        {
            throw new MemoryConflictException(
                "A semantic memory with this key and domain already exists (active or pending review).");
        }

        await AssertEventOwnedByUserAsync(userId, eventId, cancellationToken).ConfigureAwait(false);
        var at = DateTimeOffset.UtcNow;
        await using var tx = await db.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);
        try
        {
            var created = SemanticMemory.CreateInitial(
                userId,
                key,
                claim,
                confidence,
                auth,
                domain,
                at,
                initialStatus);
            db.SemanticMemories.Add(created);
            try
            {
                await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                throw new MemoryConflictException(
                    "A semantic memory with this key and domain already exists (active or pending review).");
            }

            var link = MemoryEvidence.Link(
                userId,
                created.Id,
                eventId,
                evidenceStrength,
                evidenceReason,
                at);
            db.MemoryEvidences.Add(link);
            try
            {
                await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                throw new MemoryConflictException(
                    "Could not create evidence: duplicate or conflicting row.");
            }
            catch (DbUpdateException)
            {
                throw new MemoryDomainException("Could not link evidence to the new semantic memory.");
            }

            await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
            return created;
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    public async Task<SemanticMemory?> FindActiveOrPendingByKeyDomainAsync(
        int userId,
        string key,
        string? domain,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        var k = key.Trim();
        var d = NormalizeDomain(domain);
        return await db.SemanticMemories
            .Where(
                s => s.UserId == userId &&
                    s.Key.ToLower() == k.ToLower() &&
                    (s.Status == SemanticMemoryStatus.Active || s.Status == SemanticMemoryStatus.PendingReview) &&
                    (d == null
                        ? s.Domain == null
                        : s.Domain != null && s.Domain.ToLower() == d.ToLower()))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<SemanticMemory>> FindSimilarByKeyOrDomainAsync(
        int userId,
        string? keySubstring,
        string? domain,
        int take,
        CancellationToken cancellationToken = default)
    {
        var t = Math.Clamp(take, 1, 64);
        var q = db.SemanticMemories
            .Where(
                s => s.UserId == userId &&
                    (s.Status == SemanticMemoryStatus.Active || s.Status == SemanticMemoryStatus.PendingReview));
        var d = NormalizeDomain(domain);
        if (d is not null)
        {
            q = q.Where(
                s => s.Domain != null && s.Domain.ToLower() == d.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(keySubstring))
        {
            var sub = keySubstring.Trim();
            q = q.Where(s => s.Key.ToLower().Contains(sub.ToLower()));
        }

        return await q
            .OrderByDescending(s => s.UpdatedAt)
            .Take(t)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<SemanticMemory?> GetByIdAsync(
        long id,
        int userId,
        CancellationToken cancellationToken = default) =>
        await db.SemanticMemories
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.Id == id && s.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<SemanticMemory>> ListForUserAsync(
        int userId,
        bool includePendingReview = true,
        CancellationToken cancellationToken = default)
    {
        var q = db.SemanticMemories
            .AsNoTracking()
            .Where(s => s.UserId == userId);
        if (includePendingReview)
        {
            q = q.Where(
                s => s.Status == SemanticMemoryStatus.Active || s.Status == SemanticMemoryStatus.PendingReview);
        }
        else
        {
            q = q.Where(s => s.Status == SemanticMemoryStatus.Active);
        }

        return await q
            .OrderBy(s => s.Key)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<SemanticMemory> RejectAsync(
        long id,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var row = await LoadMutableAsync(id, userId, cancellationToken).ConfigureAwait(false);
        var at = DateTimeOffset.UtcNow;
        row.MarkRejected(at);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return row;
    }

    public async Task<SemanticMemory> SetConfidenceAsync(
        long id,
        int userId,
        double newConfidence,
        bool fromInferredSource,
        CancellationToken cancellationToken = default)
    {
        var row = await LoadMutableAsync(id, userId, cancellationToken).ConfigureAwait(false);
        if (fromInferredSource)
        {
            row.ThrowIfInferredMutationBlocked();
        }

        var at = DateTimeOffset.UtcNow;
        row.SetConfidence(newConfidence, at);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return row;
    }

    private static string? NormalizeDomain(string? domain) =>
        string.IsNullOrWhiteSpace(domain) ? null : domain.Trim();

    private async Task<SemanticMemory> LoadMutableAsync(
        long id,
        int userId,
        CancellationToken cancellationToken) =>
        await db.SemanticMemories
            .FirstOrDefaultAsync(
                s => s.Id == id && s.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false)
        ?? throw new MemoryDomainException("Semantic memory was not found for this user.");

    private async Task AssertEventOwnedByUserAsync(
        int userId,
        long eventId,
        CancellationToken cancellationToken)
    {
        var ok = await db.MemoryEvents
            .AnyAsync(e => e.Id == eventId && e.UserId == userId, cancellationToken)
            .ConfigureAwait(false);
        if (!ok)
        {
            throw new MemoryDomainException("The referenced event id does not exist for this user.");
        }
    }

    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation;
}
