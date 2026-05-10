using Platform.Domain.Features.News;

namespace Platform.Application.Abstractions.News;

public interface INewsIngestRepository
{
    /// <summary>Inserts when <paramref name="urlHash" /> is new; otherwise returns duplicate.</summary>
    Task<(bool Created, string Id)> TryInsertAsync(NewsItem item, string urlHash, CancellationToken cancellationToken = default);
}
