using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Pgvector;
using Platform.Application.Abstractions.Memory.Embeddings;
using Platform.Application.Features.Memory.Embeddings;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.Memory.Embeddings;

public sealed class EfMemoryEmbeddingUpsertService(
    PlatformDbContext db,
    IMemoryEmbeddingGenerator generator,
    IOptionsMonitor<DocumentMemoryChunkingOptions> chunkOptions) : IMemoryEmbeddingUpsertService
{
    public async Task<MemoryEmbeddingUpsertOutcome> UpsertForMemoryItemAsync(
        int userId,
        long memoryItemId,
        string embeddingModelKey,
        string? embeddingModelVersion,
        CancellationToken cancellationToken = default)
    {
        var modelKey = string.IsNullOrWhiteSpace(embeddingModelKey)
            ? generator.ModelKey
            : embeddingModelKey.Trim();
        var item = await db.MemoryItems
            .FirstOrDefaultAsync(x => x.Id == memoryItemId && x.UserId == userId, cancellationToken)
            .ConfigureAwait(false);
        if (item is null)
        {
            throw new MemoryDomainException("Memory item was not found for this user.");
        }

        if (generator.Dimensions <= 0)
        {
            throw new MemoryDomainException("Embedding generator is not configured.");
        }

        var chunks = DocumentMemoryChunkBuilder.BuildChunks(item, chunkOptions.CurrentValue);
        var at = DateTimeOffset.UtcNow;
        var version = string.IsNullOrWhiteSpace(embeddingModelVersion)
            ? null
            : embeddingModelVersion.Trim();

        await db.MemoryEmbeddings
            .Where(
                x => x.UserId == userId &&
                    x.MemoryItemId == memoryItemId &&
                    x.EmbeddingModelKey == modelKey)
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);

        MemoryEmbedding? first = null;
        foreach (var ch in chunks)
        {
            var floats = await generator
                .TryEmbedRecallQueryAsync(ch.CanonicalText, cancellationToken)
                .ConfigureAwait(false);
            if (floats is null)
            {
                throw new MemoryDomainException("Embedding generator is not available or returned no vector.");
            }

            if (floats.Length != generator.Dimensions)
            {
                throw new MemoryDomainException("Embedding generator dimensions do not match the configured vector width.");
            }

            var hash = MemoryEmbeddingCanonicalText.Sha256Hex(ch.CanonicalText);
            var vector = new Vector(floats);
            var row = new MemoryEmbedding
            {
                UserId = userId,
                MemoryItemId = memoryItemId,
                EmbeddingModelKey = modelKey,
                EmbeddingModelVersion = version,
                Dimensions = floats.Length,
                ContentSha256 = hash,
                ChunkIndex = ch.ChunkIndex,
                EmbeddedText = ch.EmbeddedTextForStorage,
                Embedding = vector,
                CreatedAt = at,
                UpdatedAt = at,
            };
            db.MemoryEmbeddings.Add(row);
            first ??= row;
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (first is null || chunks.Count == 0)
        {
            throw new MemoryDomainException("No embedding chunks were produced.");
        }

        return new MemoryEmbeddingUpsertOutcome(first.Id, chunks.Count);
    }
}
