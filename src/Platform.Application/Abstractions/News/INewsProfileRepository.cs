using Platform.Domain.Features.News;

namespace Platform.Application.Abstractions.News;

public interface INewsProfileRepository
{
    Task<bool> ExistsAsync(int userId, CancellationToken cancellationToken = default);
    Task UpsertAsync(NewsUserProfile profile, CancellationToken cancellationToken = default);
}
