namespace Platform.Application.Abstractions.Memory.Semantic;

public interface IMemorySemanticMergeService
{
    Task<long> MergeApprovedAsync(
        int userId,
        IReadOnlyList<long> sourceSemanticIds,
        long canonicalSemanticId,
        string resultingClaim,
        string? domain,
        DateTimeOffset at,
        CancellationToken cancellationToken = default);
}
