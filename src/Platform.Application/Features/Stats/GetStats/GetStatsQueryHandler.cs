using Platform.Application.Abstractions.Stats;
using Platform.Contracts.V1;

namespace Platform.Application.Features.Stats.GetStats;

public sealed class GetStatsQueryHandler(IStatsReadModelSource source)
{
    public async Task<StatsPayloadDto> HandleAsync(
        GetStatsQuery _,
        CancellationToken cancellationToken = default) =>
        await source.GetAsync(cancellationToken).ConfigureAwait(false);
}
