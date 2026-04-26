using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions.Memory.Review;
using Platform.Application.Features.Memory.Review;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;
using Platform.Domain.Features.Memory.ValueObjects;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.Memory.Review;

public sealed class EfMemoryReviewService(PlatformDbContext db) : IMemoryReviewService
{
    public async Task<MemoryReviewQueueItem> CreatePendingAsync(
        MemoryReviewQueueItem item,
        CancellationToken cancellationToken = default)
    {
        db.MemoryReviewQueueItems.Add(item);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return item;
    }

    public async Task<IReadOnlyList<MemoryReviewQueueItem>> ListPendingAsync(
        int userId,
        CancellationToken cancellationToken = default) =>
        await db.MemoryReviewQueueItems
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Status == MemoryReviewStatus.Pending)
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public Task<MemoryReviewQueueItem?> GetByIdForUserAsync(
        long reviewItemId,
        int userId,
        CancellationToken cancellationToken = default) =>
        db.MemoryReviewQueueItems
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == reviewItemId && x.UserId == userId, cancellationToken);

    public async Task<long?> ApproveAsync(
        long reviewItemId,
        int userId,
        string? reviewNotes,
        CancellationToken cancellationToken = default)
    {
        await using var tx = await db.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);
        try
        {
            var row = await db.MemoryReviewQueueItems
                .FirstOrDefaultAsync(x => x.Id == reviewItemId && x.UserId == userId, cancellationToken)
                .ConfigureAwait(false);
            if (row is null)
            {
                throw new MemoryDomainException("Review item was not found for this user.");
            }

            if (row.Status != MemoryReviewStatus.Pending)
            {
                throw new MemoryDomainException("Only pending review items can be approved.");
            }

            var at = DateTimeOffset.UtcNow;
            long? semanticId = null;
            switch (row.ProposalType)
            {
                case MemoryReviewProposalType.NewSemantic:
                    var payload = MemoryReviewProposalJson.ParseNewSemantic(row.ProposedChangeJson);
                    semanticId = await UpsertSemanticFromNewSemanticProposalAsync(
                        userId,
                        payload,
                        at,
                        cancellationToken)
                        .ConfigureAwait(false);
                    break;
                case MemoryReviewProposalType.AdjustConfidence:
                case MemoryReviewProposalType.MergeDuplicate:
                case MemoryReviewProposalType.Unspecified:
                default:
                    throw new MemoryDomainException(
                        $"Proposal type {row.ProposalType} is not supported for approval in v1.");
            }

            row.Approve(at, semanticId, reviewNotes);
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
            return semanticId;
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    public async Task RejectAsync(
        long reviewItemId,
        int userId,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        var row = await db.MemoryReviewQueueItems
            .FirstOrDefaultAsync(x => x.Id == reviewItemId && x.UserId == userId, cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
        {
            throw new MemoryDomainException("Review item was not found for this user.");
        }

        row.Reject(DateTimeOffset.UtcNow, reason);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdatePendingAsync(
        long reviewItemId,
        int userId,
        string? title,
        string? summary,
        string? proposedChangeJson,
        CancellationToken cancellationToken = default)
    {
        var row = await db.MemoryReviewQueueItems
            .FirstOrDefaultAsync(x => x.Id == reviewItemId && x.UserId == userId, cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
        {
            throw new MemoryDomainException("Review item was not found for this user.");
        }

        row.ApplyPendingEdits(title, summary, proposedChangeJson, DateTimeOffset.UtcNow);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> HasPendingWithEvidenceSubstringAsync(
        int userId,
        string evidenceSubstring,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(evidenceSubstring))
        {
            return false;
        }

        var texts = await db.MemoryReviewQueueItems
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Status == MemoryReviewStatus.Pending)
            .Select(x => x.EvidenceJson)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return texts.Any(
            t => t is not null && t.Contains(evidenceSubstring, StringComparison.Ordinal));
    }

    private async Task<long> UpsertSemanticFromNewSemanticProposalAsync(
        int userId,
        NewSemanticMemoryProposalV1 payload,
        DateTimeOffset at,
        CancellationToken cancellationToken)
    {
        var key = payload.Key.Trim();
        var claim = payload.Claim.Trim();
        var domain = string.IsNullOrWhiteSpace(payload.Domain) ? null : payload.Domain.Trim();
        var conf = MemoryValueConstraints.Clamp01(payload.InitialConfidence);
        var existing = await db.SemanticMemories
            .Where(
                s => s.UserId == userId &&
                    s.Key.ToLower() == key.ToLower() &&
                    (s.Status == SemanticMemoryStatus.Active || s.Status == SemanticMemoryStatus.PendingReview))
            .OrderByDescending(s => s.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        if (existing is not null)
        {
            existing.ApplyUserApprovedRevision(
                claim,
                conf,
                AuthorityWeight.UserApprovedSemantic,
                at);
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return existing.Id;
        }

        var created = SemanticMemory.CreateInitial(
            userId,
            key,
            claim,
            conf,
            AuthorityWeight.UserApprovedSemantic,
            domain,
            at);
        db.SemanticMemories.Add(created);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return created.Id;
    }
}
