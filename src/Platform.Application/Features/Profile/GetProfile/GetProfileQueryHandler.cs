using Platform.Application.Abstractions.Profile;
using Platform.Contracts.V1;

namespace Platform.Application.Features.Profile.GetProfile;

public sealed class GetProfileQueryHandler(IProfileReadRepository profile)
{
    public async Task<UserProfileDto> HandleAsync(
        GetProfileQuery _,
        CancellationToken cancellationToken = default) =>
        await profile.GetProfileAsync(cancellationToken).ConfigureAwait(false);
}
