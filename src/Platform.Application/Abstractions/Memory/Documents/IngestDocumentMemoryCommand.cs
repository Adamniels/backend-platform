namespace Platform.Application.Abstractions.Memory.Documents;

public sealed record IngestDocumentMemoryCommand(
    int UserId,
    string Title,
    string Content,
    string? SourceType,
    string? ProjectId,
    string? Domain,
    bool IndexEmbeddings);

public sealed record IngestDocumentMemoryResult(
    long MemoryItemId,
    int EmbeddingChunksWritten,
    bool EmbeddingsIndexingAttempted);
