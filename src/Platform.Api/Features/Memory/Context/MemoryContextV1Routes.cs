using Platform.Application.Features.Memory.Context.GetMemoryContextV1;
using Platform.Contracts.V1.Memory;

namespace Platform.Api.Features.Memory.Context;

public static class MemoryContextV1Routes
{
    public static void Map(RouteGroupBuilder v1) =>
        v1.MapPost(
                "memory/context",
                async (GetMemoryContextV1Request body, PostMemoryContextRequestHandler h, CancellationToken ct) =>
                {
                    var res = await h
                        .HandleAsync(body, ct)
                        .ConfigureAwait(false);
                    return Results.Ok(res);
                })
            .DisableAntiforgery();
}
