using Platform.Application.Features.SideLearning.ListTopics;

namespace Platform.Api.Features.SideLearning;

public static class SideLearningV1Routes
{
    public static void Map(RouteGroupBuilder v1) =>
        v1.MapGet(
            "side-learning/topics",
            async (ListSideLearningTopicsQueryHandler h, CancellationToken ct) =>
                Results.Ok(
                    await h
                        .HandleAsync(new ListSideLearningTopicsQuery(), ct)
                        .ConfigureAwait(false)));
}
