using Platform.Domain.Features.Memory;

namespace Platform.Domain.Features.Memory.ValueObjects;

/// <summary>Event payload for append to episodic log before a row id exists (orchestrated by application from handlers).</summary>
public sealed record UncommittedMemoryEvent(
    int UserId,
    string EventType,
    string? Domain,
    string? WorkflowId,
    string? ProjectId,
    string? PayloadJson,
    DateTimeOffset OccurredAt)
{
    public static UncommittedMemoryEvent CreateForIngest(
        int userId,
        string eventType,
        string? domain,
        string? workflowId,
        string? projectId,
        string? payloadJson,
        DateTimeOffset occurredAt,
        DateTimeOffset systemNow)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new MemoryDomainException("EventType is required.");
        }

        if (occurredAt > systemNow.AddMinutes(1))
        {
            throw new MemoryDomainException("OccurredAt is too far in the future relative to the system clock.");
        }

        return new UncommittedMemoryEvent(
            userId,
            eventType.Trim(),
            string.IsNullOrWhiteSpace(domain) ? null : domain.Trim(),
            string.IsNullOrWhiteSpace(workflowId) ? null : workflowId.Trim(),
            string.IsNullOrWhiteSpace(projectId) ? null : projectId.Trim(),
            payloadJson,
            occurredAt);
    }
}
