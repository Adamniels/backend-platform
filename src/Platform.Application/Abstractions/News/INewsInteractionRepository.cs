using Platform.Domain.Features.News;

namespace Platform.Application.Abstractions.News;

public interface INewsInteractionRepository
{
    Task InsertAsync(NewsInteraction interaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all interactions for a user recorded on or after <paramref name="since"/>,
    /// ordered by <see cref="NewsInteraction.RecordedAt"/> descending.
    /// </summary>
    Task<IReadOnlyList<NewsInteraction>> GetRecentAsync(
        int userId,
        DateTimeOffset since,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the mean <see cref="NewsInteraction.DwellSeconds"/> across all
    /// <see cref="NewsInteractionType.Read"/> interactions for a user.
    /// Returns <see langword="null"/> when the user has no read interactions yet.
    /// </summary>
    Task<double?> GetAverageDwellSecondsAsync(int userId, CancellationToken cancellationToken = default);
}
