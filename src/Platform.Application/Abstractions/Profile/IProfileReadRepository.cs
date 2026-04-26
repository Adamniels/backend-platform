using Platform.Contracts.V1;

namespace Platform.Application.Abstractions.Profile;

public interface IProfileReadRepository
{
    Task<UserProfileDto> GetProfileAsync(CancellationToken cancellationToken = default);
    Task<UserSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default);
}
