namespace Platform.Contracts.V1.Memory;

public sealed class SemanticMemoryEvidenceV1Item
{
    public long EventId { get; set; }

    public string EventType { get; set; } = "";

    public double Strength { get; set; }

    public string? Note { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public string Polarity { get; set; } = "Support";

    public string SourceKind { get; set; } = "SystemHeuristic";

    public double ReliabilityWeight { get; set; }

    public string? SourceId { get; set; }

    public string? SchemaVersion { get; set; }

    public string? ProvenanceJson { get; set; }
}
