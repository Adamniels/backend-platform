using Platform.Application.Features.News;
using Platform.Application.Features.News.Embed;
using Platform.Application.Features.News.Ingest;
using Platform.Application.Features.News.Profile;
using Platform.Application.Features.WorkflowRuns.StartWorkflowRun;
using Platform.Contracts.V1.News;

namespace Platform.Api.Features.News.Internal;

public static class InternalNewsV1Routes
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/internal/v1/news")
            .WithTags("Internal News Worker");

        group.MapPost(
                "items",
                async (
                    IngestNewsItemV1Request body,
                    IngestNewsItemCommandHandler handler,
                    CancellationToken ct) =>
                {
                    var cmd = new IngestNewsItemCommand(
                        body.Title,
                        body.Url,
                        body.Source,
                        body.Body,
                        body.Author,
                        body.PublishedAt,
                        body.SourceFeedUrl);
                    var res = await handler.HandleAsync(cmd, ct).ConfigureAwait(false);
                    return Results.Ok(res);
                })
            .DisableAntiforgery();

        group.MapPost(
                "intelligence/runs",
                async (
                    TriggerNewsIntelligenceWorkflowV1Request? body,
                    StartWorkflowRunCommandHandler handler,
                    CancellationToken ct) =>
                {
                    var name = string.IsNullOrWhiteSpace(body?.Name)
                        ? "News intelligence (manual)"
                        : body!.Name.Trim();
                    var cmd = new StartWorkflowRunCommand(
                        name,
                        NewsIntelligenceWorkflowTypes.WorkflowTypeName,
                        null);
                    var res = await handler.HandleAsync(cmd, ct).ConfigureAwait(false);
                    return Results.Ok(res);
                })
            .DisableAntiforgery();

        group.MapPost(
                "items/{id}/embed",
                async (
                    string id,
                    EmbedNewsItemCommandHandler handler,
                    CancellationToken ct) =>
                {
                    var result = await handler
                        .HandleAsync(new EmbedNewsItemCommand(id), ct)
                        .ConfigureAwait(false);

                    var status = result switch
                    {
                        EmbedNewsItemResult.Embedded => "embedded",
                        EmbedNewsItemResult.Skipped  => "skipped",
                        _                            => "error",
                    };
                    return Results.Ok(new EmbedNewsItemV1Response(status));
                })
            .DisableAntiforgery();

        group.MapPost(
                "profile/seed",
                async (
                    SeedNewsProfileV1Request body,
                    SeedNewsProfileCommandHandler handler,
                    CancellationToken ct) =>
                {
                    var result = await handler
                        .HandleAsync(new SeedNewsProfileCommand(body.UserId), ct)
                        .ConfigureAwait(false);

                    var status = result switch
                    {
                        SeedNewsProfileResult.Seeded => "seeded",
                        SeedNewsProfileResult.Exists => "exists",
                        _                            => "error",
                    };
                    return Results.Ok(new SeedNewsProfileV1Response(status));
                })
            .DisableAntiforgery();

        group.MapPost(
                "profile/update-from-interactions",
                async (
                    UpdateNewsProfileV1Request body,
                    UpdateNewsProfileCommandHandler handler,
                    CancellationToken ct) =>
                {
                    var result = await handler
                        .HandleAsync(new UpdateNewsProfileCommand(body.UserId, body.WindowDays ?? 7), ct)
                        .ConfigureAwait(false);

                    var status = result switch
                    {
                        UpdateNewsProfileResult.Updated   => "updated",
                        UpdateNewsProfileResult.NoData    => "no-data",
                        UpdateNewsProfileResult.NoProfile => "no-profile",
                        _                                 => "error",
                    };
                    return Results.Ok(new UpdateNewsProfileV1Response(status));
                })
            .DisableAntiforgery();
    }
}
