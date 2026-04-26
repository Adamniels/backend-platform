namespace Platform.Application.Abstractions.Memory.Embeddings;

/// <summary>Governed upsert of embeddings: always anchored to an existing <c>memory_items</c> row owned by the user.</summary>
public interface IMemoryEmbeddingUpsertService
{
    /// <summary>Recomputes embedding from the current memory item title+content and upserts <c>memory_embeddings</c>.</summary>
    Task<long> UpsertForMemoryItemAsync(
        int userId,
        long memoryItemId,
        string embeddingModelKey,
        string? embeddingModelVersion,
        CancellationToken cancellationToken = default);
}
