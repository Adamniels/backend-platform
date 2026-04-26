namespace Platform.Application.Abstractions.Memory.Embeddings;

/// <summary>Produces query embeddings for recall. Implementations may call an external model or a deterministic dev stub.</summary>
public interface IMemoryEmbeddingGenerator
{
    /// <summary>Stable logical key stored on <c>memory_embeddings.EmbeddingModelKey</c> for the vectors produced.</summary>
    string ModelKey { get; }

    /// <summary>Vector width (e.g. 1536). Must match stored embeddings when searching.</summary>
    int Dimensions { get; }

    /// <summary>Returns <see langword="null"/> when embeddings are unavailable (vector recall is skipped).</summary>
    Task<float[]?> TryEmbedRecallQueryAsync(string? text, CancellationToken cancellationToken = default);
}
