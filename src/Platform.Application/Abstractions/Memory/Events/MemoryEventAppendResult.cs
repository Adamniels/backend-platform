namespace Platform.Application.Abstractions.Memory.Events;

public readonly record struct MemoryEventAppendResult(
    long Id,
    DateTimeOffset OccurredAt,
    DateTimeOffset CreatedAt);
