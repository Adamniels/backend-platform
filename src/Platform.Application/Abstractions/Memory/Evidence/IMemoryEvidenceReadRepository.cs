namespace Platform.Application.Abstractions.Memory.Evidence;

public interface IMemoryEvidenceReadRepository
{
    Task<bool> ExistsForSemanticAndEventAsync(
        int userId,
        long semanticMemoryId,
        long eventId,
        CancellationToken cancellationToken = default);
}
