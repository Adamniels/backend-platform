namespace Platform.Contracts.V1.Memory;

public sealed record MemoryItemSummaryV1Dto(
    long Id,
    string Title,
    string MemoryType,
    string Status);
