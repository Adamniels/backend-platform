namespace Platform.Contracts.V1.Memory;

public sealed record MemoryContextShellV1Dto(
    string AssemblyStage,
    IReadOnlyList<MemoryItemSummaryV1Dto> ItemSamples,
    IReadOnlyList<SemanticMemorySummaryV1Dto> SemanticSamples);
