namespace Platform.Domain.Features.Memory;

/// <summary>Legacy UI seed table; will be removed when governed memory ships with migrations.</summary>
public sealed class MemoryInsight
{
    public int Id { get; set; }
    public string Label { get; set; } = "";
    public string Content { get; set; } = "";
    public int Strength { get; set; }
    public bool Confirmed { get; set; }
}
