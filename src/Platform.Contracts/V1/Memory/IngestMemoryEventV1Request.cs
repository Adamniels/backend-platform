namespace Platform.Contracts.V1.Memory;

public sealed class IngestMemoryEventV1Request
{
    public string EventType { get; set; } = "";
    public string? Domain { get; set; }
    public string? WorkflowId { get; set; }
    public string? ProjectId { get; set; }
    public string? PayloadJson { get; set; }
    public int? UserId { get; set; }
    public DateTimeOffset? OccurredAt { get; set; }
}
