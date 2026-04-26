using Platform.Contracts.V1.Memory;

namespace Platform.Application.Abstractions.Memory.Semantic;

public interface ISemanticMemoryReadRepository
{
    Task<IReadOnlyList<SemanticMemorySummaryV1Dto>> ListSummariesForPrincipalAsync(
        int principalId,
        CancellationToken cancellationToken = default);
}
