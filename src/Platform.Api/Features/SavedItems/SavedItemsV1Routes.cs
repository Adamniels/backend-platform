using Platform.Application.Features.SavedItems.ListSavedItems;

namespace Platform.Api.Features.SavedItems;

public static class SavedItemsV1Routes
{
    public static void Map(RouteGroupBuilder v1) =>
        v1.MapGet(
            "saved-items",
            async (ListSavedItemsQueryHandler h, CancellationToken ct) =>
                Results.Ok(
                    await h
                        .HandleAsync(new ListSavedItemsQuery(), ct)
                        .ConfigureAwait(false)));
}
