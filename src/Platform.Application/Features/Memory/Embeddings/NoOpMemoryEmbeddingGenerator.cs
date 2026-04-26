using Platform.Application.Abstractions.Memory.Embeddings;

namespace Platform.Application.Features.Memory.Embeddings;

/// <summary>Disables vector recall when no embedding backend is configured.</summary>
public sealed class NoOpMemoryEmbeddingGenerator : IMemoryEmbeddingGenerator
{
    public string ModelKey => "none";

    public int Dimensions => 0;

    public Task<float[]?> TryEmbedRecallQueryAsync(string? text, CancellationToken cancellationToken = default) =>
        Task.FromResult<float[]?>(null);
}
