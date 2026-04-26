using Platform.Domain.Features.Memory;

namespace Platform.Domain.Features.Memory.Entities;

/// <summary>Append-only episodic event (ingestion enforces no update/delete at persistence level later).</summary>
public sealed class MemoryEvent
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public MemoryUser? User { get; set; }

    public string EventType { get; set; } = "";
    public string? Domain { get; set; }
    public string? WorkflowId { get; set; }
    public string? ProjectId { get; set; }
    public string? PayloadJson { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public static MemoryEvent Create(
        int userId,
        string eventType,
        string? domain,
        string? workflowId,
        string? projectId,
        string? payloadJson,
        DateTimeOffset occurredAt,
        DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new MemoryDomainException("EventType is required for a memory event.");
        }

        if (occurredAt > createdAt.AddMinutes(1))
        {
            throw new MemoryDomainException("OccurredAt cannot be meaningfully after CreatedAt (clock skew).");
        }

        return new MemoryEvent
        {
            UserId = userId,
            EventType = eventType.Trim(),
            Domain = string.IsNullOrWhiteSpace(domain) ? null : domain.Trim(),
            WorkflowId = string.IsNullOrWhiteSpace(workflowId) ? null : workflowId.Trim(),
            ProjectId = string.IsNullOrWhiteSpace(projectId) ? null : projectId.Trim(),
            PayloadJson = payloadJson,
            OccurredAt = occurredAt,
            CreatedAt = createdAt,
        };
    }
}
