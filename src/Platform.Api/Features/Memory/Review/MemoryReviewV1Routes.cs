using Platform.Application.Features.Memory.ReviewQueue.ApproveItem;
using Platform.Application.Features.Memory.ReviewQueue.CreateItem;
using Platform.Application.Features.Memory.ReviewQueue.ListPending;
using Platform.Application.Features.Memory.ReviewQueue.PatchItem;
using Platform.Application.Features.Memory.ReviewQueue.RejectItem;
using Platform.Contracts.V1.Memory;

namespace Platform.Api.Features.Memory.Review;

public static class MemoryReviewV1Routes
{
    public static void Map(RouteGroupBuilder v1)
    {
        v1.MapGet(
            "memory/review-queue",
            async (int? userId, ListMemoryReviewQueueQueryHandler h, CancellationToken ct) =>
                Results.Ok(
                    await h
                        .HandleAsync(new ListMemoryReviewQueueQuery(userId ?? 0), ct)
                        .ConfigureAwait(false)));

        v1.MapPost(
                "memory/review-queue",
                async (CreateMemoryReviewQueueItemV1Request body, CreateReviewQueueItemCommandHandler h, CancellationToken ct) =>
                {
                    var cmd = new CreateReviewQueueItemCommand(
                        body.UserId ?? 0,
                        body.ProposalType,
                        body.Title,
                        body.Summary,
                        body.ProposedChangeJson,
                        body.EvidenceJson,
                        body.Priority);
                    return Results.Ok(await h.HandleAsync(cmd, ct).ConfigureAwait(false));
                })
            .DisableAntiforgery();

        v1.MapPost(
                "memory/review-queue/{id:long}/approve",
                async (
                    long id,
                    int? userId,
                    ApproveMemoryReviewQueueItemV1Request? body,
                    ApproveReviewQueueItemCommandHandler h,
                    CancellationToken ct) =>
                {
                    var cmd = new ApproveReviewQueueItemCommand(
                        id,
                        userId ?? 0,
                        body?.ReviewNotes);
                    return Results.Ok(await h.HandleAsync(cmd, ct).ConfigureAwait(false));
                })
            .DisableAntiforgery();

        v1.MapPost(
                "memory/review-queue/{id:long}/reject",
                async (
                    long id,
                    int? userId,
                    RejectMemoryReviewQueueItemV1Request? body,
                    RejectReviewQueueItemCommandHandler h,
                    CancellationToken ct) =>
                {
                    var cmd = new RejectReviewQueueItemCommand(id, userId ?? 0, body?.Reason);
                    await h.HandleAsync(cmd, ct).ConfigureAwait(false);
                    return Results.NoContent();
                })
            .DisableAntiforgery();

        v1.MapPatch(
                "memory/review-queue/{id:long}",
                async (
                    long id,
                    int? userId,
                    PatchMemoryReviewQueueItemV1Request body,
                    PatchReviewQueueItemCommandHandler h,
                    CancellationToken ct) =>
                {
                    var cmd = new PatchReviewQueueItemCommand(
                        id,
                        userId ?? 0,
                        body.Title,
                        body.Summary,
                        body.ProposedChangeJson);
                    return Results.Ok(await h.HandleAsync(cmd, ct).ConfigureAwait(false));
                })
            .DisableAntiforgery();
    }
}
