namespace Platform.Contracts.V1.Memory;

public sealed class UpsertMemoryEmbeddingV1Request
{
    public int? UserId { get; set; }
    public long MemoryItemId { get; set; }

    /// <summary>Defaults to the configured generator model key when empty.</summary>
    public string? EmbeddingModelKey { get; set; }

    public string? EmbeddingModelVersion { get; set; }
}

public sealed class UpsertMemoryEmbeddingV1Response
{
    public long EmbeddingRowId { get; set; }
}
