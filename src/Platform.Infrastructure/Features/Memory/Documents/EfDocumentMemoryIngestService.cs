using Platform.Application.Abstractions.Memory.Documents;
using Platform.Application.Abstractions.Memory.Embeddings;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;
using Platform.Domain.Features.Memory.ValueObjects;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.Memory.Documents;

public sealed class EfDocumentMemoryIngestService(
    PlatformDbContext db,
    IMemoryEmbeddingUpsertService embeddingUpsert) : IDocumentMemoryIngestService
{
    public async Task<IngestDocumentMemoryResult> IngestAsync(
        IngestDocumentMemoryCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = command.UserId is 0 ? MemoryUser.DefaultId : command.UserId;
        var at = DateTimeOffset.UtcNow;
        var auth = AuthorityWeight.FromDoubleClamped(0.62).Value;
        var item = MemoryItem.CreateNew(
            userId,
            MemoryItemType.Document,
            command.Title,
            command.Content,
            command.SourceType ?? "document-memory-v1",
            auth,
            0.55,
            at,
            command.ProjectId,
            command.Domain);
        item.PromoteToActive(at);
        db.MemoryItems.Add(item);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var chunksWritten = 0;
        if (command.IndexEmbeddings)
        {
            try
            {
                var outcome = await embeddingUpsert
                    .UpsertForMemoryItemAsync(userId, item.Id, "", null, cancellationToken)
                    .ConfigureAwait(false);
                chunksWritten = outcome.ChunksWritten;
            }
            catch (MemoryDomainException)
            {
                // Item remains; embeddings require a configured generator.
            }
        }

        return new IngestDocumentMemoryResult(item.Id, chunksWritten, command.IndexEmbeddings);
    }
}
