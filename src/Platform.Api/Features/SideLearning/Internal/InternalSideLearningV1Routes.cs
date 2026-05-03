using Platform.Application.Features.SideLearning.Internal.PostReflectionInsights;
using Platform.Application.Features.SideLearning.Internal.PostSessionContent;
using Platform.Application.Features.SideLearning.Internal.PostTopicProposals;
using Platform.Contracts.V1.SideLearning;

namespace Platform.Api.Features.SideLearning.Internal;

public static class InternalSideLearningV1Routes
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/internal/v1/side-learning")
            .WithTags("Internal Side Learning Worker");

        group.MapPost(
                "sessions/{id}/proposals",
                async (string id, PostSideLearningTopicProposalsV1Request body, PostSideLearningTopicProposalsCommandHandler h, CancellationToken ct) =>
                {
                    try
                    {
                        await h.HandleAsync(new PostSideLearningTopicProposalsCommand(id, body), ct).ConfigureAwait(false);
                        return Results.NoContent();
                    }
                    catch (InvalidOperationException ex)
                    {
                        return Results.BadRequest(new { error = ex.Message });
                    }
                })
            .DisableAntiforgery();

        group.MapPost(
                "sessions/{id}/session-content",
                async (string id, PostSideLearningSessionContentV1Request body, PostSideLearningSessionContentCommandHandler h, CancellationToken ct) =>
                {
                    try
                    {
                        await h.HandleAsync(new PostSideLearningSessionContentCommand(id, body), ct).ConfigureAwait(false);
                        return Results.NoContent();
                    }
                    catch (InvalidOperationException ex)
                    {
                        return Results.BadRequest(new { error = ex.Message });
                    }
                })
            .DisableAntiforgery();

        group.MapPost(
                "sessions/{id}/reflection-insights",
                async (string id, PostSideLearningReflectionInsightsV1Request body, PostSideLearningReflectionInsightsCommandHandler h, CancellationToken ct) =>
                {
                    try
                    {
                        await h.HandleAsync(new PostSideLearningReflectionInsightsCommand(id, body), ct).ConfigureAwait(false);
                        return Results.NoContent();
                    }
                    catch (InvalidOperationException ex)
                    {
                        return Results.BadRequest(new { error = ex.Message });
                    }
                })
            .DisableAntiforgery();
    }
}
