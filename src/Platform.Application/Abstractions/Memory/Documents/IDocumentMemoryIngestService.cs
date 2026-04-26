namespace Platform.Application.Abstractions.Memory.Documents;

public interface IDocumentMemoryIngestService
{
    Task<IngestDocumentMemoryResult> IngestAsync(
        IngestDocumentMemoryCommand command,
        CancellationToken cancellationToken = default);
}
