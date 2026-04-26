using Platform.Contracts.V1.Memory;

namespace Platform.Application.Abstractions.Memory.Evidence;

public interface IMemoryEvidenceReadRepository
{
    Task<bool> ExistsForSemanticAndEventAsync(
        int userId,
        long semanticMemoryId,
        long eventId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SemanticMemoryEvidenceV1Item>> ListForSemanticAsync(
        int userId,
        long semanticMemoryId,
        int take,
        CancellationToken cancellationToken = default);
}
