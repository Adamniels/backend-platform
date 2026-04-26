using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Abstractions.Memory.Consolidation;

public interface IMemoryConsolidationRunRepository
{
    Task<MemoryConsolidationRun?> GetSnapshotByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task<MemoryConsolidationRun?> GetTrackedByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task AddAsync(MemoryConsolidationRun run, CancellationToken cancellationToken = default);

    Task SaveTrackedAsync(MemoryConsolidationRun run, CancellationToken cancellationToken = default);
}
