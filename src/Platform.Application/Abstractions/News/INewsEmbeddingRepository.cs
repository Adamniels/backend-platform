using Platform.Domain.Features.News;

namespace Platform.Application.Abstractions.News;

public interface INewsEmbeddingRepository
{
    Task<bool> ExistsAsync(string newsItemId, string modelKey, CancellationToken cancellationToken = default);
    Task UpsertAsync(NewsItemEmbedding embedding, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads embeddings for a set of article IDs using the given model key.
    /// Articles that have no embedding are silently omitted from the result.
    /// </summary>
    Task<IReadOnlyList<NewsItemEmbedding>> GetByNewsItemIdsAsync(
        IEnumerable<string> newsItemIds,
        string modelKey,
        CancellationToken cancellationToken = default);
}
