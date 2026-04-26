using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions.Memory.Consolidation;
using Platform.Domain.Features.Memory.Entities;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.Memory.Consolidation;

public sealed class EfMemoryConsolidationRunRepository(PlatformDbContext db) : IMemoryConsolidationRunRepository
{
    public Task<MemoryConsolidationRun?> GetSnapshotByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default) =>
        db.MemoryConsolidationRuns
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);

    public Task<MemoryConsolidationRun?> GetTrackedByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default) =>
        db.MemoryConsolidationRuns
            .AsTracking()
            .FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);

    public async Task AddAsync(MemoryConsolidationRun run, CancellationToken cancellationToken = default)
    {
        db.MemoryConsolidationRuns.Add(run);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task SaveTrackedAsync(MemoryConsolidationRun run, CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
