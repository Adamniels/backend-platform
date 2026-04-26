using FluentValidation;
using Platform.Application.Abstractions.Memory.Documents;
using Platform.Contracts.V1.Memory;

namespace Platform.Application.Features.Memory.Documents.IngestDocumentMemory;

public sealed class IngestDocumentMemoryCommandHandler(
    IValidator<IngestDocumentMemoryCommand> validator,
    IDocumentMemoryIngestService ingest)
{
    public async Task<IngestDocumentMemoryV1Response> HandleAsync(
        IngestDocumentMemoryV1Request request,
        CancellationToken cancellationToken = default)
    {
        var cmd = new IngestDocumentMemoryCommand(
            request.UserId ?? 0,
            request.Title ?? "",
            request.Content ?? "",
            request.SourceType,
            request.ProjectId,
            request.Domain,
            request.IndexEmbeddings ?? true);
        await validator.ValidateAndThrowAsync(cmd, cancellationToken).ConfigureAwait(false);
        var result = await ingest.IngestAsync(cmd, cancellationToken).ConfigureAwait(false);
        return new IngestDocumentMemoryV1Response
        {
            MemoryItemId = result.MemoryItemId,
            EmbeddingChunksWritten = result.EmbeddingChunksWritten,
            EmbeddingsIndexingAttempted = result.EmbeddingsIndexingAttempted,
        };
    }
}
