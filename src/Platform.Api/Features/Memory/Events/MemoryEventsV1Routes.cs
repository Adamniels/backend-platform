using Microsoft.AspNetCore.Http;
using Platform.Application.Features.Memory.Events.IngestEvent;
using Platform.Application.Features.Memory.Events.ListMemoryEvents;
using Platform.Contracts.V1.Memory;

namespace Platform.Api.Features.Memory.Events;

public static class MemoryEventsV1Routes
{
    public static void Map(RouteGroupBuilder v1)
    {
        v1.MapGet(
            "memory/events",
            async (int? userId, int? take, ListMemoryEventsQueryHandler h, CancellationToken ct) =>
            {
                var list = await h
                    .HandleAsync(new ListMemoryEventsQuery(userId ?? 0, take ?? 80), ct)
                    .ConfigureAwait(false);
                return Results.Ok(list);
            });

        v1.MapPost(
                "memory/events",
                async (
                    IngestMemoryEventV1Request body,
                    IngestMemoryEventCommandHandler handler,
                    CancellationToken ct) =>
                {
                    var command = new IngestMemoryEventCommand(
                        body.EventType,
                        body.Domain,
                        body.WorkflowId,
                        body.ProjectId,
                        string.IsNullOrWhiteSpace(body.PayloadJson) ? null : body.PayloadJson,
                        body.UserId ?? 0,
                        body.OccurredAt);
                    var result = await handler
                        .HandleAsync(command, ct)
                        .ConfigureAwait(false);
                    return Results.Json(result, statusCode: StatusCodes.Status201Created);
                })
            .DisableAntiforgery();
    }
}
