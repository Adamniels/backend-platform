namespace Platform.Contracts.V1.Memory;

public sealed class SemanticMemoryEvidenceV1Item
{
    public long EventId { get; set; }

    public string EventType { get; set; } = "";

    public double Strength { get; set; }

    public string? Note { get; set; }

    public DateTimeOffset OccurredAt { get; set; }
}
