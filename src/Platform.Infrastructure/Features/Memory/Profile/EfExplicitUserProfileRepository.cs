using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions.Memory.Profile;
using Platform.Domain.Features.Memory.Entities;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.Memory.Profile;

public sealed class EfExplicitUserProfileRepository(PlatformDbContext db) : IExplicitUserProfileRepository
{
    public Task<ExplicitUserProfile?> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default) =>
        db.ExplicitUserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

    public async Task<ExplicitUserProfile> SaveAsync(
        ExplicitUserProfile model,
        CancellationToken cancellationToken = default)
    {
        if (model.Id == 0)
        {
            db.ExplicitUserProfiles.Add(model);
        }
        else
        {
            db.ExplicitUserProfiles.Update(model);
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return model;
    }
}
