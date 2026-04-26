namespace Platform.Application.Features.Memory.ReviewQueue.ApproveItem;

public sealed record ApproveReviewQueueItemCommand(
    long ReviewItemId,
    int UserId,
    string? ReviewNotes);
