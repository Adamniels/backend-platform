using System.Security.Cryptography;
using System.Text;
using Platform.Application.Abstractions.Memory.Embeddings;

namespace Platform.Application.Features.Memory.Embeddings;

/// <summary>
/// Deterministic pseudo-embeddings for dev/tests (no external API). Output is L2-normalized for cosine similarity.
/// </summary>
public sealed class DeterministicRecallEmbeddingGenerator : IMemoryEmbeddingGenerator
{
    public const string DefaultModelKey = "deterministic-recall-v1";

    public string ModelKey => DefaultModelKey;

    public int Dimensions => MemoryVectorRecallConstants.EmbeddingDimensions;

    public Task<float[]?> TryEmbedRecallQueryAsync(string? text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Task.FromResult<float[]?>(null);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<float[]?>(EmbedText(text.Trim(), MemoryVectorRecallConstants.EmbeddingDimensions));
    }

    public static float[] EmbedText(string text, int dimensions)
    {
        var vec = new float[dimensions];
        var seed = Encoding.UTF8.GetBytes(text);
        var block = seed;
        using var sha = SHA256.Create();
        for (var i = 0; i < dimensions; i++)
        {
            if (i % 32 == 0)
            {
                block = sha.ComputeHash(block);
            }

            vec[i] = block[i % 32] / 255f - 0.5f;
        }

        var norm = Math.Sqrt(vec.Sum(static x => x * x));
        if (norm < 1e-8)
        {
            vec[0] = 1f;
            return vec;
        }

        for (var i = 0; i < dimensions; i++)
        {
            vec[i] /= (float)norm;
        }

        return vec;
    }
}
