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

    Task<IReadOnlyList<SideLearningSession>> ListForUserAsync(
        int userId,
        int take,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
