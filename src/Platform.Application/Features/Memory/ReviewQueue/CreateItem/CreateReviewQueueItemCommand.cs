namespace Platform.Application.Features.Memory.ReviewQueue.CreateItem;

public sealed record CreateReviewQueueItemCommand(
    int UserId,
    string ProposalType,
    string Title,
    string Summary,
    string? ProposedChangeJson,
    string? EvidenceJson,
    int Priority);
