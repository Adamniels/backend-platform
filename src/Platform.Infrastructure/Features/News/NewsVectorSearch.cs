using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using Platform.Application.Abstractions.Memory.Embeddings;
using Platform.Application.Abstractions.News;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.News;

/// <summary>
/// Ranks news articles by cosine similarity against a blended query vector built from up to three
/// profile components: long-term (always present), short-term (14-day behavioral snapshot), and
/// active context (declared interests and projects). Missing optional components fall back to the
/// long-term vector so the weights always sum to 1.0 before normalization.
/// </summary>
public sealed class NewsVectorSearch(
    PlatformDbContext db,
    IMemoryEmbeddingGenerator embeddingGenerator) : INewsVectorSearch
{
    // Blend weights — must sum to 1.0.
    private const double WeightLongTerm     = 0.70;
    private const double WeightShortTerm    = 0.20;
    private const double WeightActiveContext = 0.10;

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

        var lt  = profile.LongTermEmbedding.ToArray();
        var st  = profile.ShortTermEmbedding?.ToArray();
        var ac  = profile.ActiveContextEmbedding?.ToArray();
        var dim = embeddingGenerator.Dimensions;

        // Build the blended query vector.
        // When an optional component is absent, its weight falls back to long-term
        // so the total contribution is always 1.0 before normalization.
        var q = new double[dim];
        for (var i = 0; i < dim; i++)
        {
            q[i]  = WeightLongTerm      * lt[i];
            q[i] += WeightShortTerm     * (st != null ? st[i] : lt[i]);
            q[i] += WeightActiveContext * (ac != null ? ac[i] : lt[i]);
        }

        // Normalize to a unit vector before passing to pgvector cosine distance.
        var magnitude = Math.Sqrt(q.Sum(x => x * x));
        if (magnitude < 1e-10)
            return [];
        for (var i = 0; i < dim; i++)
            q[i] /= magnitude;

        var qv = new Vector(q.Select(d => (float)d).ToArray());
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
