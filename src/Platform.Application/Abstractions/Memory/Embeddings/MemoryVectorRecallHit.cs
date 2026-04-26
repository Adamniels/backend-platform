namespace Platform.Application.Abstractions.Memory.Embeddings;

/// <summary>One governed memory item (or document chunk) surfaced by vector recall; always tied to <c>memory_items</c>.</summary>
public sealed record MemoryVectorRecallHit(
    long MemoryItemId,
    int ChunkIndex,
    string MemoryType,
    string Title,
    string ContentPreview,
    double CosineSimilarity,
    double AuthorityWeight,
    string EmbeddingModelKey,
    string? ProjectId,
    string? Domain,
    string SourceType);
