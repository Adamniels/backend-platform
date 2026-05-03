using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions.SideLearning;
using Platform.Domain.Features.SideLearning;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.SideLearning;

public sealed class SideLearningSessionRepository(PlatformDbContext db) : ISideLearningSessionRepository
{
    public async Task AddAsync(SideLearningSession session, CancellationToken cancellationToken = default)
    {
        db.SideLearningSessions.Add(session);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task<SideLearningSession?> GetTrackedForUserAsync(
        string id,
        int userId,
        CancellationToken cancellationToken = default) =>
        db.SideLearningSessions.SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);

    public Task<SideLearningSession?> GetTrackedByIdAsync(string id, CancellationToken cancellationToken = default) =>
        db.SideLearningSessions.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<SideLearningSession>> ListForUserByLifecycleAsync(
        int userId,
        string lifecycle,
        int take,
        CancellationToken cancellationToken = default)
    {
        var q = db.SideLearningSessions.AsNoTracking().Where(x => x.UserId == userId);
        if (string.Equals(lifecycle, "ongoing", StringComparison.OrdinalIgnoreCase))
        {
            q = q.Where(x =>
                x.Phase != SideLearningSessionPhase.Completed && x.Phase != SideLearningSessionPhase.Failed);
        }
        else
        {
            q = q.Where(x =>
                x.Phase == SideLearningSessionPhase.Completed || x.Phase == SideLearningSessionPhase.Failed);
        }

        return await q
            .OrderByDescending(x => x.UpdatedAt)
            .ThenByDescending(x => x.Id)
            .Take(take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);

    public async Task<bool> DeleteForUserAsync(
        string id,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var session = await GetTrackedForUserAsync(id, userId, cancellationToken).ConfigureAwait(false);
        if (session is null)
        {
            return false;
        }

        db.SideLearningSessions.Remove(session);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
