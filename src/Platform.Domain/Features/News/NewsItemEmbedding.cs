using Pgvector;

namespace Platform.Domain.Features.News;

public sealed class NewsItemEmbedding
{
    public long Id { get; set; }

    /// <summary>FK to NewsItems.Id — cascade delete.</summary>
    public string NewsItemId { get; set; } = "";
    public NewsItem? NewsItem { get; set; }

    /// <summary>Stable logical key for the model used, e.g. "text-embedding-3-small".</summary>
    public string EmbeddingModelKey { get; set; } = "";

    /// <summary>Vector dimensionality — 1536 for text-embedding-3-small.</summary>
    public int Dimensions { get; set; }

    /// <summary>The pgvector embedding column.</summary>
    public Vector Embedding { get; set; } = null!;

    public DateTimeOffset EmbeddedAt { get; set; }
}
