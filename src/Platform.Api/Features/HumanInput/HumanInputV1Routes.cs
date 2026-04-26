using Platform.Application.Features.HumanInput.ListItems;

namespace Platform.Api.Features.HumanInput;

public static class HumanInputV1Routes
{
    public static void Map(RouteGroupBuilder v1) =>
        v1.MapGet(
            "human-input/items",
            async (ListHumanInputQueryHandler h, CancellationToken ct) =>
                Results.Ok(
                    await h
                        .HandleAsync(new ListHumanInputQuery(), ct)
                        .ConfigureAwait(false)));
}
