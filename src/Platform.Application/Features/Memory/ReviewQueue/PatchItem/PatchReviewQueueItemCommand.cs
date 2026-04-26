namespace Platform.Application.Features.Memory.ReviewQueue.PatchItem;

public sealed record PatchReviewQueueItemCommand(
    long ReviewItemId,
    int UserId,
    string? Title,
    string? Summary,
    string? ProposedChangeJson);
