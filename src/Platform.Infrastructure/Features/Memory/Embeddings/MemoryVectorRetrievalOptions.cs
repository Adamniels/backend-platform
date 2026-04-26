namespace Platform.Infrastructure.Features.Memory.Embeddings;

public sealed class MemoryVectorRetrievalOptions
{
    public const string SectionName = "MemoryVector";

    /// <summary>When true, uses <see cref="Platform.Application.Features.Memory.Embeddings.DeterministicRecallEmbeddingGenerator"/> (no external API).</summary>
    public bool UseDeterministicEmbeddingGenerator { get; set; }
}
