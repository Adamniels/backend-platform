namespace Platform.Domain.Features.Memory.Entities;

public sealed class MemoryEvidence
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public MemoryUser? User { get; set; }

    public long SemanticMemoryId { get; set; }
    public SemanticMemory? SemanticMemory { get; set; }
    public long EventId { get; set; }
    public MemoryEvent? SourceEvent { get; set; }
    public double Strength { get; set; }
    public string? Reason { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
