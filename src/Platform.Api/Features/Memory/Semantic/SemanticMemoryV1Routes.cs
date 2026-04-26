using Microsoft.AspNetCore.Http;
using Platform.Application.Features.Memory.Semantic.ArchiveSemanticMemory;
using Platform.Application.Features.Memory.Semantic.AttachSemanticMemoryEvidence;
using Platform.Application.Features.Memory.Semantic.CreateSemanticMemory;
using Platform.Application.Features.Memory.Semantic.FindSimilarSemanticMemories;
using Platform.Application.Features.Memory.Semantic.GetSemanticMemory;
using Platform.Application.Features.Memory.Semantic.ListSemanticMemories;
using Platform.Application.Features.Memory.Semantic.RejectSemanticMemory;
using Platform.Application.Features.Memory.Semantic.UpdateSemanticMemoryConfidence;
using Platform.Contracts.V1.Memory;

namespace Platform.Api.Features.Memory.Semantic;

public static class SemanticMemoryV1Routes
{
    public static void Map(RouteGroupBuilder v1)
    {
        v1.MapGet(
            "memory/semantics",
            async (
                int? userId,
                bool? includePending,
                ListSemanticMemoriesQueryHandler h,
                CancellationToken ct) =>
            {
                var q = new ListSemanticMemoriesQuery(
                    userId ?? 0,
                    includePending ?? true);
                return Results.Ok(await h.HandleAsync(q, ct).ConfigureAwait(false));
            });

        v1.MapGet(
            "memory/semantics/find",
            async (
                int? userId,
                string? key,
                string? domain,
                int? take,
                FindSimilarSemanticMemoriesQueryHandler h,
                CancellationToken ct) =>
            {
                var q = new FindSimilarSemanticMemoriesQuery(
                    userId ?? 0,
                    key,
                    domain,
                    take ?? 16);
                return Results.Ok(await h.HandleAsync(q, ct).ConfigureAwait(false));
            });

        v1.MapGet(
            "memory/semantics/{id:long}",
            async (
                long id,
                int? userId,
                GetSemanticMemoryQueryHandler h,
                CancellationToken ct) =>
            {
                var res = await h
                    .HandleAsync(new GetSemanticMemoryQuery(id, userId ?? 0), ct)
                    .ConfigureAwait(false);
                return res is null
                    ? Results.NotFound()
                    : Results.Ok(res);
            });

        v1.MapPost(
                "memory/semantics",
                async (
                    CreateSemanticMemoryV1Request body,
                    CreateSemanticMemoryCommandHandler h,
                    CancellationToken ct) =>
                {
                    var cmd = new CreateSemanticMemoryCommand(
                        body.UserId ?? 0,
                        body.Key,
                        body.Claim,
                        body.Confidence,
                        body.AuthorityWeight,
                        body.Domain,
                        body.Status,
                        body.EventId,
                        body.EvidenceStrength,
                        body.EvidenceReason);
                    var res = await h
                        .HandleAsync(cmd, ct)
                        .ConfigureAwait(false);
                    return Results.Json(res, statusCode: StatusCodes.Status201Created);
                })
            .DisableAntiforgery();

        v1.MapPut(
                "memory/semantics/{id:long}/confidence",
                async (
                    long id,
                    UpdateSemanticMemoryConfidenceV1Request body,
                    int? userId,
                    UpdateSemanticMemoryConfidenceCommandHandler h,
                    CancellationToken ct) =>
                {
                    var res = await h
                        .HandleAsync(
                            new UpdateSemanticMemoryConfidenceCommand(
                                id,
                                userId ?? body.UserId ?? 0,
                                body.Confidence,
                                body.FromInferredSource),
                            ct)
                        .ConfigureAwait(false);
                    return Results.Ok(res);
                })
            .DisableAntiforgery();

        v1.MapPost(
                "memory/semantics/{id:long}/evidence",
                async (
                    long id,
                    AttachSemanticMemoryEvidenceV1Request body,
                    int? userId,
                    AttachSemanticMemoryEvidenceCommandHandler h,
                    CancellationToken ct) =>
                {
                    var res = await h
                        .HandleAsync(
                            new AttachSemanticMemoryEvidenceCommand(
                                id,
                                userId ?? body.UserId ?? 0,
                                body.EventId,
                                body.Strength,
                                body.Reason,
                                body.FromInferredSource,
                                body.ReinforceConfidence,
                                body.ReinforceConfidenceDelta,
                                body.EventOccurredAt),
                            ct)
                        .ConfigureAwait(false);
                    return Results.Ok(res);
                })
            .DisableAntiforgery();

        v1.MapPost(
                "memory/semantics/{id:long}/archive",
                async (long id, int? userId, ArchiveSemanticMemoryCommandHandler h, CancellationToken ct) =>
                {
                    var res = await h
                        .HandleAsync(
                            new ArchiveSemanticMemoryCommand(id, userId ?? 0),
                            ct)
                        .ConfigureAwait(false);
                    return Results.Ok(res);
                })
            .DisableAntiforgery();

        v1.MapPost(
                "memory/semantics/{id:long}/reject",
                async (long id, int? userId, RejectSemanticMemoryCommandHandler h, CancellationToken ct) =>
                {
                    var res = await h
                        .HandleAsync(
                            new RejectSemanticMemoryCommand(id, userId ?? 0),
                            ct)
                        .ConfigureAwait(false);
                    return Results.Ok(res);
                })
            .DisableAntiforgery();
    }
}
