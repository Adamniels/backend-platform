namespace Platform.Domain.Features.Memory.Entities;

/// <summary>Link between a semantic claim and an episodic event (see <c>memory_evidence</c>).</summary>
public sealed class MemoryEvidence
{
    public long Id { get; set; }
    public long SemanticMemoryId { get; set; }
    public long EventId { get; set; }
    public double Strength { get; set; }
    public string? Reason { get; set; }
}
