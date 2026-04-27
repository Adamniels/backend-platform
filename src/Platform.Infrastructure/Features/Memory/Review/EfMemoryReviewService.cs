using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions.Memory.Review;
using Platform.Application.Features.Memory.Review;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;
using Platform.Infrastructure.Features.Memory.Review.Approval;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.Memory.Review;

public sealed class EfMemoryReviewService(
    PlatformDbContext db,
    IMemoryReviewApprovalHandlerResolver approvalHandlerResolver)
    : IMemoryReviewService
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

    public async Task<MemoryReviewApprovalResult> ApproveAsync(
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
            var approval = await approvalHandlerResolver
                .Resolve(row.ProposalType)
                .ApproveAsync(row, userId, at, cancellationToken)
                .ConfigureAwait(false);

            row.Approve(at, approval.SemanticMemoryId, approval.ProceduralRuleId, reviewNotes);
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
            return approval;
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

        if (proposedChangeJson is not null)
        {
            MemoryReviewProposalJson.ValidateForProposalType(row.ProposalType, proposedChangeJson);
        }

        row.ApplyPendingEdits(title, summary, proposedChangeJson, DateTimeOffset.UtcNow);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task<bool> HasPendingWithFingerprintAsync(
        int userId,
        MemoryReviewProposalType proposalType,
        string dedupFingerprint,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dedupFingerprint))
        {
            return Task.FromResult(false);
        }

        return db.MemoryReviewQueueItems
            .AsNoTracking()
            .AnyAsync(
                x => x.UserId == userId &&
                    x.ProposalType == proposalType &&
                    x.Status == MemoryReviewStatus.Pending &&
                    x.DedupFingerprint == dedupFingerprint.Trim(),
                cancellationToken);
    }
}
