using Platform.Application.Features.SideLearning.Sessions.Create;
using Platform.Application.Features.SideLearning.Sessions.Get;
using Platform.Application.Features.SideLearning.Sessions.List;
using Platform.Application.Features.SideLearning.Sessions.Progress;
using Platform.Application.Features.SideLearning.Sessions.Reflect;
using Platform.Application.Features.SideLearning.Sessions.SelectTopic;
using Platform.Contracts.V1.SideLearning;

namespace Platform.Api.Features.SideLearning;

public static class SideLearningV1Routes
{
    public static void Map(RouteGroupBuilder v1)
    {
        v1.MapPost(
                "side-learning/sessions",
                async (CreateSideLearningSessionV1Request? body, CreateSideLearningSessionCommandHandler h, CancellationToken ct) =>
                {
                    try
                    {
                        var cmd = new CreateSideLearningSessionCommand(body?.InitialPrompt);
                        var res = await h.HandleAsync(cmd, ct).ConfigureAwait(false);
                        return Results.Ok(res);
                    }
                    catch (InvalidOperationException ex)
                    {
                        return Results.BadRequest(new { error = ex.Message });
                    }
                })
            .DisableAntiforgery();

        v1.MapGet(
            "side-learning/sessions",
            async (ListSideLearningSessionsQueryHandler h, CancellationToken ct) =>
                Results.Ok(await h.HandleAsync(new ListSideLearningSessionsQuery(), ct).ConfigureAwait(false)));

        v1.MapGet(
            "side-learning/sessions/{id}",
            async (string id, GetSideLearningSessionQueryHandler h, CancellationToken ct) =>
            {
                var dto = await h.HandleAsync(new GetSideLearningSessionQuery(id), ct).ConfigureAwait(false);
                return dto is null ? Results.NotFound() : Results.Ok(dto);
            });

        v1.MapPost(
                "side-learning/sessions/{id}/select-topic",
                async (string id, SelectSideLearningTopicV1Request body, SelectSideLearningTopicCommandHandler h, CancellationToken ct) =>
                {
                    try
                    {
                        await h
                            .HandleAsync(
                                new SelectSideLearningTopicCommand(id, body.TopicTitle ?? "", body.Feedback),
                                ct)
                            .ConfigureAwait(false);
                        return Results.NoContent();
                    }
                    catch (InvalidOperationException ex)
                    {
                        return Results.BadRequest(new { error = ex.Message });
                    }
                })
            .DisableAntiforgery();

        v1.MapPost(
                "side-learning/sessions/{id}/progress",
                async (string id, UpdateSideLearningProgressV1Request body, UpdateSideLearningProgressCommandHandler h, CancellationToken ct) =>
                {
                    try
                    {
                        await h
                            .HandleAsync(
                                new UpdateSideLearningProgressCommand(id, body.SectionId ?? "", body.Completed),
                                ct)
                            .ConfigureAwait(false);
                        return Results.NoContent();
                    }
                    catch (InvalidOperationException ex)
                    {
                        return Results.BadRequest(new { error = ex.Message });
                    }
                })
            .DisableAntiforgery();

        v1.MapPost(
                "side-learning/sessions/{id}/reflect",
                async (string id, SubmitSideLearningReflectionV1Request body, SubmitSideLearningReflectionCommandHandler h, CancellationToken ct) =>
                {
                    try
                    {
                        await h
                            .HandleAsync(new SubmitSideLearningReflectionCommand(id, body.Reflection ?? ""), ct)
                            .ConfigureAwait(false);
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
