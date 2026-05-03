using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Platform.Application.Configuration;
using Platform.Application.Features.Memory.Consolidation.Nightly;
using Platform.Application.Features.Memory.Context.GetMemoryContextV1;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory;

namespace Platform.Api.Features.Memory.Internal;

public static class InternalMemoryV1Routes
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/internal/v1/memory")
            .WithTags("Internal Memory Worker");

        group.MapPost(
                "context",
                async (
                    GetMemoryContextV1Request? body,
                    PostMemoryContextRequestHandler handler,
                    IOptions<PlatformWorkerOptions> workerOptions,
                    CancellationToken ct) =>
                {
                    var w = body ?? new GetMemoryContextV1Request();
                    var userId = w.UserId is null or 0 ? workerOptions.Value.PrimaryUserId : w.UserId.Value;
                    var merged = new GetMemoryContextV1Request
                    {
                        UserId = userId,
                        TaskDescription = w.TaskDescription,
                        WorkflowType = w.WorkflowType,
                        ProjectId = w.ProjectId,
                        Domain = w.Domain,
                        IncludeVectorRecall = w.IncludeVectorRecall,
                    };
                    var res = await handler.HandleAsync(merged, ct).ConfigureAwait(false);
                    return Results.Ok(res);
                })
            .DisableAntiforgery();

        group.MapPost(
                "consolidation/nightly",
                async (
                    ExecuteNightlyMemoryConsolidationV1Request? body,
                    ExecuteNightlyMemoryConsolidationCommandHandler handler,
                    IOptions<PlatformWorkerOptions> workerOptions,
                    CancellationToken ct) =>
                {
                    var w = body ?? new ExecuteNightlyMemoryConsolidationV1Request();
                    var userId = w.UserId is null or 0
                        ? workerOptions.Value.PrimaryUserId
                        : w.UserId.Value;
                    var windowEnd = w.WindowEndExclusiveUtc ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);
                    var idempotencyKey = string.IsNullOrWhiteSpace(w.IdempotencyKey)
                        ? $"nightly-{userId}-{windowEnd:yyyy-MM-dd}"
                        : w.IdempotencyKey.Trim();
                    try
                    {
                        var cmd = new ExecuteNightlyMemoryConsolidationCommand(
                            userId,
                            windowEnd,
                            idempotencyKey);
                        var res = await handler
                            .HandleAsync(cmd, ct)
                            .ConfigureAwait(false);
                        return Results.Ok(res);
                    }
                    catch (MemoryConflictException ex)
                    {
                        return Results.Problem(
                            title: "Conflict",
                            detail: ex.Message,
                            statusCode: StatusCodes.Status409Conflict);
                    }
                })
            .DisableAntiforgery();
    }
}
