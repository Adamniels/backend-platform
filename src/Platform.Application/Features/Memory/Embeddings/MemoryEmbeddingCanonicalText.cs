using System.Security.Cryptography;
using System.Text;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Features.Memory.Embeddings;

public static class MemoryEmbeddingCanonicalText
{
    public static string ForMemoryItem(MemoryItem item) =>
        ForTitleAndContent(item.Title, item.Content);

    public static string ForTitleAndContent(string title, string? content) =>
        $"{title.Trim()}\u001F{content ?? string.Empty}";

    /// <summary>Canonical text for one document chunk (distinct from whole-item <see cref="ForTitleAndContent"/>).</summary>
    public static string ForDocumentChunk(string titleTrimmed, int chunkIndex, string chunkBody) =>
        $"{titleTrimmed.Trim()}\u001Fchunk:{chunkIndex}\u001F{chunkBody}";

    public static string Sha256Hex(string canonicalText)
    {
        var bytes = Encoding.UTF8.GetBytes(canonicalText);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
