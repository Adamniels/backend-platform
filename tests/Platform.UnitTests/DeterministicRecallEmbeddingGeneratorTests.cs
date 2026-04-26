using Platform.Application.Features.Memory.Embeddings;

namespace Platform.UnitTests;

public sealed class DeterministicRecallEmbeddingGeneratorTests
{
    [Fact]
    public void EmbedText_is_unit_normalized_and_deterministic()
    {
        var a = DeterministicRecallEmbeddingGenerator.EmbedText("hello world", MemoryVectorRecallConstants.EmbeddingDimensions);
        var b = DeterministicRecallEmbeddingGenerator.EmbedText("hello world", MemoryVectorRecallConstants.EmbeddingDimensions);
        Assert.Equal(MemoryVectorRecallConstants.EmbeddingDimensions, a.Length);
        Assert.Equal(a, b);
        var norm = Math.Sqrt(a.Sum(static x => x * x));
        Assert.InRange(norm, 0.99, 1.01);
    }
}
