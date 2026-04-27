using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions.Memory.Evidence;
using Platform.Contracts.V1.Memory;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.Memory.Evidence;

public sealed class EfMemoryEvidenceReadRepository(PlatformDbContext db) : IMemoryEvidenceReadRepository
{
    public Task<bool> ExistsForSemanticAndEventAsync(
        int userId,
        long semanticMemoryId,
        long eventId,
        CancellationToken cancellationToken = default) =>
        db.MemoryEvidences
            .AsNoTracking()
            .AnyAsync(
                e => e.UserId == userId && e.SemanticMemoryId == semanticMemoryId && e.EventId == eventId,
                cancellationToken);

    public async Task<IReadOnlyList<SemanticMemoryEvidenceV1Item>> ListForSemanticAsync(
        int userId,
        long semanticMemoryId,
        int take,
        CancellationToken cancellationToken = default)
    {
        var n = Math.Clamp(take, 1, 100);
        var rows = await (
                from e in db.MemoryEvidences.AsNoTracking()
                join ev in db.MemoryEvents.AsNoTracking() on e.EventId equals ev.Id
                where e.UserId == userId &&
                    e.SemanticMemoryId == semanticMemoryId &&
                    ev.UserId == userId
                orderby ev.OccurredAt descending, e.Id descending
                select new SemanticMemoryEvidenceV1Item
                {
                    EventId = ev.Id,
                    EventType = ev.EventType,
                    Strength = e.Strength,
                    Note = e.Reason,
                    OccurredAt = ev.OccurredAt,
                    Polarity = e.Polarity.ToString(),
                    SourceKind = e.SourceKind.ToString(),
                    ReliabilityWeight = e.ReliabilityWeight,
                    SourceId = e.SourceId,
                    SchemaVersion = e.SchemaVersion,
                    ProvenanceJson = e.ProvenanceJson,
                })
            .Take(n)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return rows;
    }
}
