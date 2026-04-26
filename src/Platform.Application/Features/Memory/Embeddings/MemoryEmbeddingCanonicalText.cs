using System.Security.Cryptography;
using System.Text;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Features.Memory.Embeddings;

public static class MemoryEmbeddingCanonicalText
{
    public static string ForMemoryItem(MemoryItem item) =>
        $"{item.Title.Trim()}\u001F{item.Content ?? string.Empty}";

    public static string Sha256Hex(string canonicalText)
    {
        var bytes = Encoding.UTF8.GetBytes(canonicalText);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
