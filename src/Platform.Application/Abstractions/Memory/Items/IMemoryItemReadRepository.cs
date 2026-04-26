using Platform.Contracts.V1.Memory;

namespace Platform.Application.Abstractions.Memory.Items;

public interface IMemoryItemReadRepository
{
    Task<IReadOnlyList<MemoryItemSummaryV1Dto>> ListSummariesForPrincipalAsync(
        int principalId,
        CancellationToken cancellationToken = default);
}
