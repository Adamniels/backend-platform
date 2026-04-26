namespace Platform.Domain.Features.Memory.ValueObjects;

public sealed record UncommittedMemoryEvent(
    int UserId,
    string EventType,
    string? Domain,
    string? WorkflowId,
    string? ProjectId,
    string? PayloadJson,
    DateTimeOffset OccurredAt);
