namespace Platform.Contracts.V1.Memory;

public sealed record MemoryReviewQueueItemV1Dto(
    long Id,
    string Title,
    string Status,
    string ProposalType,
    int Priority,
    string CreatedAtIso);
