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

    public async Task<IReadOnlyList<SideLearningSession>> ListForUserAsync(
        int userId,
        int take,
        CancellationToken cancellationToken = default) =>
        await db.SideLearningSessions.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
