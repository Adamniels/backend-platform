namespace Platform.Application.Abstractions.Memory.Embeddings;

/// <summary>Governed upsert of embeddings: always anchored to an existing <c>memory_items</c> row owned by the user.</summary>
public interface IMemoryEmbeddingUpsertService
{
    /// <summary>Recomputes embeddings from the current memory item and replaces <c>memory_embeddings</c> rows for the model key.</summary>
    Task<MemoryEmbeddingUpsertOutcome> UpsertForMemoryItemAsync(
        int userId,
        long memoryItemId,
        string embeddingModelKey,
        string? embeddingModelVersion,
        CancellationToken cancellationToken = default);
}
