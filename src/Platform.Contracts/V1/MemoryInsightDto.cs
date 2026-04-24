namespace Platform.Contracts.V1;

public sealed record MemoryInsightDto(int Id, string Label, string Content, int Strength, bool Confirmed);
