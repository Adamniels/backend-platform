using Platform.Domain.Features.Memory.ValueObjects;

namespace Platform.Application.Features.Memory.Events.IngestEvent;

public sealed record IngestMemoryEventCommand(
    string EventType,
    string? Domain = null,
    string? WorkflowId = null,
    string? ProjectId = null,
    string? PayloadJson = null,
    int PrincipalId = 0)
{
    public int ResolvedPrincipalId => PrincipalId == 0
        ? MemoryPrincipalId.SingleTenant.Value
        : PrincipalId;
}
