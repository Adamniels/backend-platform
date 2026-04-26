using Microsoft.EntityFrameworkCore;
using Pgvector;
using Platform.Application.Abstractions.Memory.Embeddings;
using Platform.Application.Features.Memory.Embeddings;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.Memory.Embeddings;

public sealed class EfMemoryEmbeddingUpsertService(
    PlatformDbContext db,
    IMemoryEmbeddingGenerator generator) : IMemoryEmbeddingUpsertService
{
    public async Task<long> UpsertForMemoryItemAsync(
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

        var canonical = MemoryEmbeddingCanonicalText.ForMemoryItem(item);
        var hash = MemoryEmbeddingCanonicalText.Sha256Hex(canonical);
        var floats = await generator.TryEmbedRecallQueryAsync(canonical, cancellationToken).ConfigureAwait(false);
        if (floats is null)
        {
            throw new MemoryDomainException("Embedding generator is not available or returned no vector.");
        }

        if (generator.Dimensions <= 0)
        {
            throw new MemoryDomainException("Embedding generator is not configured.");
        }

        if (floats.Length != generator.Dimensions)
        {
            throw new MemoryDomainException("Embedding generator dimensions do not match the configured vector width.");
        }

        var at = DateTimeOffset.UtcNow;
        var vector = new Vector(floats);
        var existing = await db.MemoryEmbeddings
            .FirstOrDefaultAsync(
                x => x.UserId == userId &&
                    x.MemoryItemId == memoryItemId &&
                    x.EmbeddingModelKey == modelKey,
                cancellationToken)
            .ConfigureAwait(false);
        if (existing is null)
        {
            var row = new MemoryEmbedding
            {
                UserId = userId,
                MemoryItemId = memoryItemId,
                EmbeddingModelKey = modelKey,
                EmbeddingModelVersion = string.IsNullOrWhiteSpace(embeddingModelVersion)
                    ? null
                    : embeddingModelVersion.Trim(),
                Dimensions = floats.Length,
                ContentSha256 = hash,
                Embedding = vector,
                CreatedAt = at,
                UpdatedAt = at,
            };
            db.MemoryEmbeddings.Add(row);
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return row.Id;
        }

        existing.Embedding = vector;
        existing.Dimensions = floats.Length;
        existing.ContentSha256 = hash;
        existing.EmbeddingModelVersion = string.IsNullOrWhiteSpace(embeddingModelVersion)
            ? null
            : embeddingModelVersion.Trim();
        existing.UpdatedAt = at;
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return existing.Id;
    }
}
