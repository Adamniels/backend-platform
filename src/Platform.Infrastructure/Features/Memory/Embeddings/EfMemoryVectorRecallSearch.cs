using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using Platform.Application.Abstractions.Memory.Embeddings;
using Platform.Application.Features.Memory.Embeddings;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.Memory.Embeddings;

public sealed class EfMemoryVectorRecallSearch(PlatformDbContext db) : IMemoryVectorRecallSearch
{
    public async Task<IReadOnlyList<MemoryVectorRecallHit>> SearchMemoryItemsAsync(
        int userId,
        float[] queryEmbedding,
        string embeddingModelKey,
        int take,
        CancellationToken cancellationToken = default)
    {
        if (take <= 0 || queryEmbedding.Length == 0)
        {
            return Array.Empty<MemoryVectorRecallHit>();
        }

        if (string.IsNullOrWhiteSpace(embeddingModelKey))
        {
            return Array.Empty<MemoryVectorRecallHit>();
        }

        if (queryEmbedding.Length != MemoryVectorRecallConstants.EmbeddingDimensions)
        {
            throw new MemoryDomainException(
                $"Query embedding length {queryEmbedding.Length} does not match expected {MemoryVectorRecallConstants.EmbeddingDimensions}.");
        }

        var qv = new Vector(queryEmbedding);
        var rows = await (
                from e in db.MemoryEmbeddings.AsNoTracking()
                join m in db.MemoryItems.AsNoTracking() on e.MemoryItemId equals m.Id
                where e.UserId == userId &&
                    m.UserId == userId &&
                    e.EmbeddingModelKey == embeddingModelKey &&
                    e.Dimensions == queryEmbedding.Length &&
                    m.Status == MemoryItemStatus.Active
                orderby e.Embedding!.CosineDistance(qv)
                select new
                {
                    e.MemoryItemId,
                    m.MemoryType,
                    m.Title,
                    m.Content,
                    m.AuthorityWeight,
                    e.EmbeddingModelKey,
                    Dist = e.Embedding!.CosineDistance(qv),
                })
            .Take(take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var hits = new List<MemoryVectorRecallHit>(rows.Count);
        foreach (var r in rows)
        {
            var sim = 1d - r.Dist;
            if (sim < 0)
            {
                sim = 0;
            }

            if (sim > 1)
            {
                sim = 1;
            }

            var preview = r.Content;
            if (!string.IsNullOrEmpty(preview) && preview.Length > 400)
            {
                preview = preview.Substring(0, 400) + "…";
            }

            hits.Add(
                new MemoryVectorRecallHit(
                    r.MemoryItemId,
                    r.MemoryType.ToString(),
                    r.Title,
                    preview ?? "",
                    sim,
                    r.AuthorityWeight,
                    r.EmbeddingModelKey));
        }

        return hits;
    }
}
