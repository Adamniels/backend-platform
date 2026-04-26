using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions.Memory.Semantic;
using Platform.Application.Features.Memory.Context;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.Memory.Semantic;

public sealed class EfSemanticMemoryReadRepository(PlatformDbContext db) : ISemanticMemoryReadRepository
{
    public async Task<IReadOnlyList<SemanticMemorySummaryV1Dto>> ListSummariesForUserAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var rows = await db.SemanticMemories
            .AsNoTracking()
            .Where(
                s => s.UserId == userId &&
                    (s.Status == SemanticMemoryStatus.Active || s.Status == SemanticMemoryStatus.PendingReview))
            .OrderBy(s => s.Key)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return rows
            .Select(
                s => new SemanticMemorySummaryV1Dto(
                    s.Id,
                    s.Key,
                    s.Claim,
                    s.Confidence,
                    MemoryContextV1Scoring.SemanticStatusString(s.Status)))
            .ToList();
    }
}
