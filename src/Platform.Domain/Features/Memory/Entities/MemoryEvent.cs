namespace Platform.Domain.Features.Memory.Entities;

/// <summary>Append-only episodic log (see <c>memory_events</c>).</summary>
public sealed class MemoryEvent
{
    public long Id { get; set; }
    public int PrincipalId { get; set; }
    public string EventType { get; set; } = "";
    public string? Domain { get; set; }
    public string? WorkflowId { get; set; }
    public string? ProjectId { get; set; }
    public string? PayloadJson { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}
