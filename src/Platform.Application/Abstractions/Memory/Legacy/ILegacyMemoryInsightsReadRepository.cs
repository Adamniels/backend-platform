using Platform.Contracts.V1;

namespace Platform.Application.Abstractions.Memory.Legacy;

public interface ILegacyMemoryInsightsReadRepository
{
    Task<IReadOnlyList<MemoryInsightDto>> ListInsightsAsync(CancellationToken cancellationToken = default);
}
