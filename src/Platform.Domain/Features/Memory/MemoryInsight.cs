namespace Platform.Domain.Features.Memory;

public sealed class MemoryInsight
{
    public int Id { get; set; }
    public string Label { get; set; } = "";
    public string Content { get; set; } = "";
    public int Strength { get; set; }
    public bool Confirmed { get; set; }
}
