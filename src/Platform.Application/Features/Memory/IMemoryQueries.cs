using Platform.Contracts.V1;

namespace Platform.Application.Features.Memory;

public interface IMemoryQueries
{
    Task<IReadOnlyList<MemoryInsightDto>> ListInsightsAsync(CancellationToken cancellationToken = default);
}
