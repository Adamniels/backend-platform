using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;
using Platform.Application.Abstractions.Memory.Embeddings;
using Platform.Application.Abstractions.News;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.News;

/// <summary>
/// Ranks news articles by cosine similarity against the user's stored interest profile vector.
/// Returns empty list when no profile exists so callers can fall back to chronological order.
/// </summary>
public sealed class NewsVectorSearch(
    PlatformDbContext db,
    IMemoryEmbeddingGenerator embeddingGenerator) : INewsVectorSearch
{
    public async Task<IReadOnlyList<NewsVectorHit>> RankByRelevanceAsync(
        int userId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var profile = await db.NewsUserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken)
            .ConfigureAwait(false);

        if (profile is null)
            return [];

        var qv = profile.LongTermEmbedding;
        var modelKey = embeddingGenerator.ModelKey;

        return await db.NewsItemEmbeddings
            .AsNoTracking()
            .Where(e => e.EmbeddingModelKey == modelKey)
            .OrderBy(e => e.Embedding.CosineDistance(qv))
            .Take(limit)
            .Select(e => new NewsVectorHit(
                e.NewsItemId,
                1.0 - (double)e.Embedding.CosineDistance(qv)))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
