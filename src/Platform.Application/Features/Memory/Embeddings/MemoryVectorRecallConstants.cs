namespace Platform.Application.Features.Memory.Embeddings;

/// <summary>Must match PostgreSQL <c>vector(N)</c> width on <c>memory_embeddings</c>.</summary>
public static class MemoryVectorRecallConstants
{
    public const int EmbeddingDimensions = 1536;
}
