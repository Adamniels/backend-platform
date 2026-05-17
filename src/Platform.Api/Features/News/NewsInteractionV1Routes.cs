using Microsoft.Extensions.Options;
using Platform.Application.Configuration;
using Platform.Application.Features.News.RecordInteraction;
using Platform.Contracts.V1.News;

namespace Platform.Api.Features.News;

public static class NewsInteractionV1Routes
{
    public static void Map(RouteGroupBuilder v1)
    {
        v1.MapPost(
                "news/interactions",
                async (
                    RecordNewsInteractionV1Request body,
                    RecordNewsInteractionCommandHandler handler,
                    IOptions<PlatformWorkerOptions> workerOptions,
                    CancellationToken ct) =>
                {
                    // Single-user system: user identity matches the primary user id throughout
                    // the news feature. When multi-user support is added, this should read from
                    // the authenticated session instead.
                    var userId = workerOptions.Value.PrimaryUserId;

                    await handler.HandleAsync(
                        new RecordNewsInteractionCommand(
                            userId,
                            (body.NewsItemId ?? "").Trim(),
                            (body.Type ?? "").Trim(),
                            body.DwellSeconds),
                        ct).ConfigureAwait(false);

                    return Results.NoContent();
                })
            .DisableAntiforgery();
    }
}
