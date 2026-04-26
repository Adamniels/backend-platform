using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions.Memory.Evidence;
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
}
