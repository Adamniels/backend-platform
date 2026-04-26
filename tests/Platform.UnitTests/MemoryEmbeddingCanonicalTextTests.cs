using Platform.Application.Features.Memory.Embeddings;

namespace Platform.UnitTests;

public sealed class MemoryEmbeddingCanonicalTextTests
{
    [Fact]
    public void Sha256Hex_is_stable_lowercase()
    {
        var h = MemoryEmbeddingCanonicalText.Sha256Hex("alpha");
        Assert.Equal(64, h.Length);
        Assert.Equal(h, MemoryEmbeddingCanonicalText.Sha256Hex("alpha"));
        Assert.True(h.All(static c => c is >= '0' and <= '9' or >= 'a' and <= 'f'));
    }
}
