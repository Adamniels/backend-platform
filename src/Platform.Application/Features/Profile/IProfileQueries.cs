using Platform.Contracts.V1;

namespace Platform.Application.Features.Profile;

public interface IProfileQueries
{
    Task<UserProfileDto> GetProfileAsync(CancellationToken cancellationToken = default);
    Task<UserSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default);
}
