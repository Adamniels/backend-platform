using Platform.Application.Features.Memory.Legacy.Insights;

namespace Platform.Api.Features.Memory.Legacy.Insights;

public static class MemoryInsightsV1Routes
{
    public static void Map(RouteGroupBuilder v1) =>
        v1.MapGet(
            "memory/insights",
            async (ListMemoryInsightsQueryHandler h, CancellationToken ct) =>
                Results.Ok(
                    await h
                        .HandleAsync(new ListMemoryInsightsQuery(), ct)
                        .ConfigureAwait(false)));
}
