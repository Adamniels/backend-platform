using Platform.Application.Abstractions.Profile;
using Platform.Contracts.V1;

namespace Platform.Application.Features.Profile.GetSettings;

public sealed class GetUserSettingsQueryHandler(IProfileReadRepository profile)
{
    public async Task<UserSettingsDto> HandleAsync(
        GetUserSettingsQuery _,
        CancellationToken cancellationToken = default) =>
        await profile.GetSettingsAsync(cancellationToken).ConfigureAwait(false);
}
