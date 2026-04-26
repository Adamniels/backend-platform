namespace Platform.Application.Features.Memory.ReviewQueue.RejectItem;

public sealed record RejectReviewQueueItemCommand(
    long ReviewItemId,
    int UserId,
    string? Reason);
