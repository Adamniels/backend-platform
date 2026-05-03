using Platform.Domain.Features.SideLearning;

namespace Platform.Application.Abstractions.SideLearning;

public interface ISideLearningSessionRepository
{
    Task AddAsync(SideLearningSession session, CancellationToken cancellationToken = default);

    Task<SideLearningSession?> GetTrackedForUserAsync(
        string id,
        int userId,
        CancellationToken cancellationToken = default);

    Task<SideLearningSession?> GetTrackedByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// <paramref name="lifecycle"/> must be <c>ongoing</c> (not completed/failed) or <c>archive</c> (completed/failed only).
    /// </summary>
    Task<IReadOnlyList<SideLearningSession>> ListForUserByLifecycleAsync(
        int userId,
        string lifecycle,
        int take,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Removes the session if it exists for the given user. Returns whether a row was deleted.</summary>
    Task<bool> DeleteForUserAsync(string id, int userId, CancellationToken cancellationToken = default);
}
