using Pgvector;

namespace Platform.Domain.Features.Memory.Entities;

/// <summary>
/// pgvector row backing governed <see cref="MemoryItem"/> recall. Embeddings are retrieval support only;
/// <see cref="MemoryItem"/> remains the source of truth for content and lifecycle.
/// </summary>
public sealed class MemoryEmbedding
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public MemoryUser? User { get; set; }

    /// <summary>FK to canonical <see cref="MemoryItem"/> (includes <see cref="MemoryItemType.Document"/> rows).</summary>
    public long MemoryItemId { get; set; }
    public MemoryItem? MemoryItem { get; set; }

    /// <summary>Logical model identifier (e.g. <c>text-embedding-3-small</c> or <c>deterministic-recall-v1</c>).</summary>
    public string EmbeddingModelKey { get; set; } = "";

    public string? EmbeddingModelVersion { get; set; }

    /// <summary>Embedding dimension count; must match <see cref="Embedding"/> column width.</summary>
    public int Dimensions { get; set; }

    /// <summary>SHA-256 hex (64 chars) of canonical embedded text for idempotency.</summary>
    public string ContentSha256 { get; set; } = "";

    public Vector Embedding { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
