using Microsoft.AspNetCore.Http;
using Platform.Application.Features.Memory.Events.IngestEvent;
using Platform.Contracts.V1.Memory;

namespace Platform.Api.Features.Memory.Events;

public static class MemoryEventsV1Routes
{
    public static void Map(RouteGroupBuilder v1) =>
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
