namespace Platform.Contracts.V1.Memory;

public sealed class IngestDocumentMemoryV1Request
{
    public int? UserId { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    /// <summary>Origin label (e.g. <c>architecture-doc</c>, <c>session-export</c>).</summary>
    public string? SourceType { get; set; }

    public string? ProjectId { get; set; }

    public string? Domain { get; set; }

    /// <summary>When true (default), runs governed embedding upsert after save (requires a configured embedding generator).</summary>
    public bool? IndexEmbeddings { get; set; }
}

public sealed class IngestDocumentMemoryV1Response
{
    public long MemoryItemId { get; set; }

    public int EmbeddingChunksWritten { get; set; }

    public bool EmbeddingsIndexingAttempted { get; set; }
}
