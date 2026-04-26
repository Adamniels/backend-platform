namespace Platform.Domain.Features.Memory.ValueObjects;

/// <summary>Payload for append-only episodic log before a DB row is assigned. Persistence wiring is not implemented yet.</summary>
public sealed record UncommittedMemoryEvent(
    MemoryPrincipalId PrincipalId,
    string EventType,
    string? Domain,
    string? WorkflowId,
    string? ProjectId,
    string? PayloadJson,
    DateTimeOffset OccurredAt);
