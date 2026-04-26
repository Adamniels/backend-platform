using Platform.Application.Features.News.ListFeed;

namespace Platform.Api.Features.News;

public static class NewsV1Routes
{
    public static void Map(RouteGroupBuilder v1) =>
        v1.MapGet(
            "news/feed",
            async (ListNewsFeedQueryHandler h, CancellationToken ct) =>
                Results.Ok(
                    await h
                        .HandleAsync(new ListNewsFeedQuery(), ct)
                        .ConfigureAwait(false)));
}
