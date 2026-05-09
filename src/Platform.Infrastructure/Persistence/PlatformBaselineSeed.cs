using Microsoft.EntityFrameworkCore;
using Platform.Domain.Features.Memory.Entities;
using Platform.Domain.Features.Profile;

namespace Platform.Infrastructure.Persistence;

/// <summary>
/// Inserts the singleton/system rows the application requires to function on an otherwise empty
/// database (the post-migration "baseline"). Safe to call repeatedly: each row is only inserted
/// when missing. No demo or mock data is added here — production code paths only.
/// </summary>
public static class PlatformBaselineSeed
{
    /// <summary>
    /// Ensures the baseline singletons exist:
    /// <see cref="MemoryUser"/> (DefaultId), <see cref="PlatformProfile"/>,
    /// and <see cref="PlatformUserSettings"/>.
    /// </summary>
    /// <param name="now">Optional timestamp used when newly inserting the <see cref="MemoryUser"/> row.
    /// Defaults to <see cref="DateTimeOffset.UtcNow"/>.</param>
    public static async Task<BaselineSeedResult> EnsureAsync(
        PlatformDbContext db,
        DateTimeOffset? now = null,
        CancellationToken cancellationToken = default)
    {
        var inserted = 0;
        var at = now ?? DateTimeOffset.UtcNow;

        if (!await db.MemoryUsers.AnyAsync(x => x.Id == MemoryUser.DefaultId, cancellationToken))
        {
            db.Add(new MemoryUser { Id = MemoryUser.DefaultId, CreatedAt = at });
            inserted++;
        }

        if (!await db.Profiles.AnyAsync(x => x.Id == PlatformProfile.SingletonKey, cancellationToken))
        {
            db.Add(new PlatformProfile
            {
                Id = PlatformProfile.SingletonKey,
                DisplayName = "You",
                Email = "you@example.com",
            });
            inserted++;
        }

        if (!await db.UserSettings.AnyAsync(x => x.Id == PlatformUserSettings.SingletonKey, cancellationToken))
        {
            db.Add(new PlatformUserSettings
            {
                Id = PlatformUserSettings.SingletonKey,
                Theme = "system",
                DigestEmail = true,
            });
            inserted++;
        }

        if (inserted > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        return new BaselineSeedResult(inserted);
    }
}

public readonly record struct BaselineSeedResult(int InsertedRowCount)
{
    public bool DatabaseWasEmpty => InsertedRowCount > 0;
}
