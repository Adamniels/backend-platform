namespace Platform.Domain.Features.Memory.Entities;

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
}
