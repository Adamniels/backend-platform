namespace Platform.Contracts.V1.Memory;

public sealed class MemoryEventV1ListItem
{
    public long Id { get; set; }

    public string EventType { get; set; } = "";

    public string? Domain { get; set; }

    public string? ProjectId { get; set; }

    public string? WorkflowId { get; set; }

    public string? PayloadPreview { get; set; }

    public DateTimeOffset OccurredAt { get; set; }
}
