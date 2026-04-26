using Platform.Application.Features.Stats.GetStats;

namespace Platform.Api.Features.Stats;

public static class StatsV1Routes
{
    public static void Map(RouteGroupBuilder v1) =>
        v1.MapGet(
            "stats",
            async (GetStatsQueryHandler h, CancellationToken ct) =>
                Results.Ok(
                    await h
                        .HandleAsync(new GetStatsQuery(), ct)
                        .ConfigureAwait(false)));
}
