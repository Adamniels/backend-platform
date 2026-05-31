using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions.News;
using Platform.Domain.Features.News;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.News;

public sealed class EfNewsProfileRepository(PlatformDbContext db) : INewsProfileRepository
{
    public async Task<bool> ExistsAsync(int userId, CancellationToken cancellationToken = default) =>
        await db.NewsUserProfiles
            .AsNoTracking()
            .AnyAsync(p => p.UserId == userId, cancellationToken)
            .ConfigureAwait(false);

    public async Task<NewsUserProfile?> GetAsync(int userId, CancellationToken cancellationToken = default) =>
        await db.NewsUserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken)
            .ConfigureAwait(false);

    public async Task UpsertAsync(NewsUserProfile profile, CancellationToken cancellationToken = default)
    {
        var existing = await db.NewsUserProfiles
            .FirstOrDefaultAsync(p => p.UserId == profile.UserId, cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            existing.LongTermEmbedding      = profile.LongTermEmbedding;
            existing.SeedText               = profile.SeedText;
            existing.UpdatedAt              = profile.UpdatedAt;
            // Phase 4 fields — nullable, only copy when the caller has set them.
            existing.ShortTermEmbedding     = profile.ShortTermEmbedding;
            existing.ShortTermUpdatedAt     = profile.ShortTermUpdatedAt;
            existing.ActiveContextEmbedding = profile.ActiveContextEmbedding;
            existing.ActiveContextUpdatedAt = profile.ActiveContextUpdatedAt;
        }
        else
        {
            db.NewsUserProfiles.Add(profile);
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
