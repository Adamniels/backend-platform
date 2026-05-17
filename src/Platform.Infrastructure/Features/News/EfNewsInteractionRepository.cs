using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions.News;
using Platform.Domain.Features.News;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.News;

public sealed class EfNewsInteractionRepository(PlatformDbContext db) : INewsInteractionRepository
{
    public async Task InsertAsync(NewsInteraction interaction, CancellationToken cancellationToken = default)
    {
        db.NewsInteractions.Add(interaction);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<NewsInteraction>> GetRecentAsync(
        int userId,
        DateTimeOffset since,
        CancellationToken cancellationToken = default) =>
        await db.NewsInteractions
            .AsNoTracking()
            .Where(i => i.UserId == userId && i.RecordedAt >= since)
            .OrderByDescending(i => i.RecordedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<double?> GetAverageDwellSecondsAsync(
        int userId,
        CancellationToken cancellationToken = default) =>
        await db.NewsInteractions
            .AsNoTracking()
            .Where(i => i.UserId == userId
                     && i.Type == NewsInteractionType.Read
                     && i.DwellSeconds != null)
            .AverageAsync(i => (double?)i.DwellSeconds, cancellationToken)
            .ConfigureAwait(false);
}
