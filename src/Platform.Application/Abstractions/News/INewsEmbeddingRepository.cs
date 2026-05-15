using Platform.Domain.Features.News;

namespace Platform.Application.Abstractions.News;

public interface INewsEmbeddingRepository
{
    Task<bool> ExistsAsync(string newsItemId, string modelKey, CancellationToken cancellationToken = default);
    Task UpsertAsync(NewsItemEmbedding embedding, CancellationToken cancellationToken = default);
}
