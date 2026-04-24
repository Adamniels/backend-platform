using Platform.Contracts.V1;

namespace Platform.Application.Features.Dashboard;

public interface IStatsQueries
{
    Task<StatsPayloadDto> GetAsync(CancellationToken cancellationToken = default);
}
