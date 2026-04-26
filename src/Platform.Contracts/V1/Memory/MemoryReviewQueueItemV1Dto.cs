namespace Platform.Contracts.V1.Memory;

public sealed record MemoryReviewQueueItemV1Dto(
    long Id,
    string Title,
    string Summary,
    string Status,
    string ProposalType,
    int Priority,
    string CreatedAtIso,
    string UpdatedAtIso,
    long? ApprovedSemanticMemoryId,
    string? RejectedReason,
    string? ResolvedAtIso,
    string? ReviewNotes,
    string? ProposedChangeJson,
    string? EvidenceJson,
    long? ApprovedProceduralRuleId);
