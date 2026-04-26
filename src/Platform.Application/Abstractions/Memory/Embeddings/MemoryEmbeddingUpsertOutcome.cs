namespace Platform.Application.Abstractions.Memory.Embeddings;

/// <summary>Result of a governed embedding write (one row for most types; multiple for chunked documents).</summary>
public readonly record struct MemoryEmbeddingUpsertOutcome(long FirstEmbeddingRowId, int ChunksWritten);
