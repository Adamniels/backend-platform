using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions.News;
using Platform.Domain.Features.News;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.News;

public sealed class EfNewsEmbeddingRepository(PlatformDbContext db) : INewsEmbeddingRepository
{
    public async Task<bool> ExistsAsync(
        string newsItemId,
        string modelKey,
        CancellationToken cancellationToken = default) =>
        await db.NewsItemEmbeddings
            .AsNoTracking()
            .AnyAsync(
                e => e.NewsItemId == newsItemId && e.EmbeddingModelKey == modelKey,
                cancellationToken)
            .ConfigureAwait(false);

    public async Task UpsertAsync(NewsItemEmbedding embedding, CancellationToken cancellationToken = default)
    {
        var existing = await db.NewsItemEmbeddings
            .FirstOrDefaultAsync(
                e => e.NewsItemId == embedding.NewsItemId
                     && e.EmbeddingModelKey == embedding.EmbeddingModelKey,
                cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            existing.Embedding = embedding.Embedding;
            existing.EmbeddedAt = embedding.EmbeddedAt;
        }
        else
        {
            db.NewsItemEmbeddings.Add(embedding);
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
