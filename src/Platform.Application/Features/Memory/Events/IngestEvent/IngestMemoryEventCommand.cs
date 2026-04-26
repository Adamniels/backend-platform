using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Features.Memory.Events.IngestEvent;

public sealed record IngestMemoryEventCommand(
    string EventType,
    string? Domain = null,
    string? WorkflowId = null,
    string? ProjectId = null,
    string? PayloadJson = null,
    int UserId = 0,
    DateTimeOffset? OccurredAt = null)
{
    public int ResolvedUserId => UserId == 0
        ? MemoryUser.DefaultId
        : UserId;
}
