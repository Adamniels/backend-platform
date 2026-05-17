using Platform.Domain.Features.News;

namespace Platform.Application.Abstractions.News;

public interface INewsProfileRepository
{
    Task<bool> ExistsAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>Returns the profile for a user, or <see langword="null"/> if none exists yet.</summary>
    Task<NewsUserProfile?> GetAsync(int userId, CancellationToken cancellationToken = default);

    Task UpsertAsync(NewsUserProfile profile, CancellationToken cancellationToken = default);
}
