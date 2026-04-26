using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Features.Memory.Embeddings;

public readonly record struct DocumentMemoryEmbeddingChunk(
    int ChunkIndex,
    string CanonicalText,
    string EmbeddedTextForStorage);

/// <summary>Builds embedding slices for <see cref="MemoryItemType.Document"/> and single-vector paths for other types.</summary>
public static class DocumentMemoryChunkBuilder
{
    public static IReadOnlyList<DocumentMemoryEmbeddingChunk> BuildChunks(
        MemoryItem item,
        DocumentMemoryChunkingOptions opts)
    {
        if (item.MemoryType != MemoryItemType.Document)
        {
            var single = MemoryEmbeddingCanonicalText.ForMemoryItem(item);
            return [new DocumentMemoryEmbeddingChunk(0, single, single)];
        }

        var title = item.Title.Trim();
        var body = item.Content ?? string.Empty;
        if (body.Length <= opts.MaxCharsBeforeChunking)
        {
            var canonical = MemoryEmbeddingCanonicalText.ForTitleAndContent(title, body);
            return [new DocumentMemoryEmbeddingChunk(0, canonical, canonical)];
        }

        var max = Math.Max(256, opts.MaxChunkBodyChars);
        var overlap = Math.Clamp(opts.OverlapChars, 0, max - 1);
        var chunks = new List<DocumentMemoryEmbeddingChunk>();
        var start = 0;
        var index = 0;
        while (start < body.Length)
        {
            var len = Math.Min(max, body.Length - start);
            var piece = body.Substring(start, len);
            var canonical = MemoryEmbeddingCanonicalText.ForDocumentChunk(title, index, piece);
            var stored = piece.Length <= opts.MaxEmbeddedTextStoredChars
                ? piece
                : piece.Substring(0, opts.MaxEmbeddedTextStoredChars);
            chunks.Add(new DocumentMemoryEmbeddingChunk(index, canonical, stored));
            index++;
            if (start + len >= body.Length)
            {
                break;
            }

            start += Math.Max(1, max - overlap);
        }

        return chunks;
    }
}
