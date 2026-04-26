using Platform.Application.Features.Memory.ListInsights;

namespace Platform.Api.Features.Memory;

public static class MemoryV1Routes
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
