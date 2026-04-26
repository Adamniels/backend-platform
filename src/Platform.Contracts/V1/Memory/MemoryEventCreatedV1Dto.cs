namespace Platform.Contracts.V1.Memory;

public sealed record MemoryEventCreatedV1Dto(
    long Id,
    DateTimeOffset OccurredAt,
    DateTimeOffset CreatedAt);
