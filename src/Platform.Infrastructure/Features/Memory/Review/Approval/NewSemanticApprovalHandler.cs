using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions.Memory.Review;
using Platform.Application.Features.Memory.Review;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;
using Platform.Domain.Features.Memory.ValueObjects;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.Memory.Review.Approval;

internal sealed class NewSemanticApprovalHandler(PlatformDbContext db) : IMemoryReviewApprovalHandler
{
    public MemoryReviewProposalType ProposalType => MemoryReviewProposalType.NewSemantic;

    public async Task<MemoryReviewApprovalResult> ApproveAsync(
        MemoryReviewQueueItem row,
        int userId,
        DateTimeOffset at,
        CancellationToken cancellationToken)
    {
        var payload = MemoryReviewProposalJson.ParseNewSemantic(row.ProposedChangeJson);
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
            existing.ApplyUserApprovedRevision(claim, conf, AuthorityWeight.UserApprovedSemantic, at);
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return new(existing.Id, null);
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
        return new(created.Id, null);
    }
}
