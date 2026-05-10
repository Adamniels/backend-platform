using System.Globalization;
using Platform.Application.Features.News;
using Platform.Application.Features.News.Ingest;
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
                    if (!DateTimeOffset.TryParse(
                            body.PublishedAt,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.RoundtripKind,
                            out var publishedAt))
                    {
                        return Results.Problem(
                            title: "Invalid publishedAt",
                            detail: "Expected an ISO-8601 date/time string.",
                            statusCode: StatusCodes.Status400BadRequest);
                    }

                    var cmd = new IngestNewsItemCommand(
                        body.Title,
                        body.Url,
                        body.Source,
                        body.Body,
                        body.Author,
                        publishedAt,
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
    }
}
