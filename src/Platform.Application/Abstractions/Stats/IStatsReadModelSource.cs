using Platform.Contracts.V1;

namespace Platform.Application.Abstractions.Stats;

public interface IStatsReadModelSource
{
    Task<StatsPayloadDto> GetAsync(CancellationToken cancellationToken = default);
}
