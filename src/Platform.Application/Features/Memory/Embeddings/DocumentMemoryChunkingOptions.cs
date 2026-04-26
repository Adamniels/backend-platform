namespace Platform.Application.Features.Memory.Embeddings;

/// <summary>Character-based chunking for long <see cref="Platform.Domain.Features.Memory.MemoryItemType.Document"/> bodies.</summary>
public sealed class DocumentMemoryChunkingOptions
{
    public const int DefaultMaxChunkBodyChars = 1800;

    public const int DefaultOverlapChars = 200;

    public const int DefaultMaxCharsBeforeChunking = 1800;

    /// <summary>Max characters per chunk body (excluding title prefix in canonical text).</summary>
    public int MaxChunkBodyChars { get; set; } = DefaultMaxChunkBodyChars;

    /// <summary>Characters of overlap between consecutive chunks.</summary>
    public int OverlapChars { get; set; } = DefaultOverlapChars;

    /// <summary>When body length is at or below this, embed as a single vector using whole-item canonical text.</summary>
    public int MaxCharsBeforeChunking { get; set; } = DefaultMaxCharsBeforeChunking;

    /// <summary>Max characters stored in <c>memory_embeddings.EmbeddedText</c> per row.</summary>
    public int MaxEmbeddedTextStoredChars { get; set; } = 32000;
}
