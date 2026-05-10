using Platform.Application.Features.News.DeleteItems;
using Platform.Application.Features.News.ListFeed;
using Platform.Contracts.V1.News;

namespace Platform.Api.Features.News;

public static class NewsV1Routes
{
    public static void Map(RouteGroupBuilder v1)
    {
        v1.MapGet(
            "news/feed",
            async (ListNewsFeedQueryHandler h, CancellationToken ct) =>
                Results.Ok(
                    await h
                        .HandleAsync(new ListNewsFeedQuery(), ct)
                        .ConfigureAwait(false)));

        v1.MapPost(
                "news/items/delete",
                async (
                    DeleteNewsItemsV1Request body,
                    DeleteNewsItemsCommandHandler handler,
                    CancellationToken ct) =>
                {
                    var ids = (body.Ids ?? Array.Empty<string>())
                        .Select(s => s.Trim())
                        .Where(s => s.Length > 0)
                        .Distinct(StringComparer.Ordinal)
                        .ToArray();
                    var res = await handler
                        .HandleAsync(new DeleteNewsItemsCommand(ids), ct)
                        .ConfigureAwait(false);
                    return Results.Ok(res);
                })
            .DisableAntiforgery();
    }
}
