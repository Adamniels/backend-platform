using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions.Profile;
using Platform.Contracts.V1;
using Platform.Domain.Features.Profile;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.Profile;

public sealed class ProfileReadRepository(PlatformDbContext db) : IProfileReadRepository
{
    public async Task<UserProfileDto> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        var p = await db.Profiles.AsNoTracking()
            .SingleAsync(x => x.Id == PlatformProfile.SingletonKey, cancellationToken);
        return new UserProfileDto(p.DisplayName, p.Email);
    }

    public async Task<UserSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var s = await db.UserSettings.AsNoTracking()
            .SingleAsync(x => x.Id == PlatformUserSettings.SingletonKey, cancellationToken);
        return new UserSettingsDto(s.Theme, s.DigestEmail);
    }
}
