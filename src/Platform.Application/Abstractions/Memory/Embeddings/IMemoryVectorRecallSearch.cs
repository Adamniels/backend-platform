namespace Platform.Application.Abstractions.Memory.Embeddings;

/// <summary>Vector similarity search over governed rows in <c>memory_embeddings</c> joined to <c>memory_items</c>.</summary>
public interface IMemoryVectorRecallSearch
{
    Task<IReadOnlyList<MemoryVectorRecallHit>> SearchMemoryItemsAsync(
        int userId,
        float[] queryEmbedding,
        string embeddingModelKey,
        int take,
        string? restrictDocumentRecallToProjectId = null,
        string? restrictDocumentRecallToDomain = null,
        CancellationToken cancellationToken = default);
}
