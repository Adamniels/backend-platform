using Platform.Contracts.V1;

namespace Platform.Application.Abstractions.Memory;

public interface IMemoryReadRepository
{
    Task<IReadOnlyList<MemoryInsightDto>> ListInsightsAsync(CancellationToken cancellationToken = default);
}
