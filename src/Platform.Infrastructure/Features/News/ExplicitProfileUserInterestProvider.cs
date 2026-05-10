using Platform.Application.Abstractions.Memory.Profile;
using Platform.Application.Abstractions.Memory.Users;
using Platform.Application.Abstractions.News;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Infrastructure.Features.News;

public sealed class ExplicitProfileUserInterestProvider(
    IExplicitUserProfileRepository profile,
    IMemoryUserContextResolver userResolver) : IUserInterestProvider
{
    public async Task<UserInterestSnapshot> GetInterestsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var id = userResolver.Resolve(userId);
        var row = await profile
            .GetByUserIdAsync(id, cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
        {
            return new UserInterestSnapshot(
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<UserInterestProjectSnapshot>());
        }

        var projects = ExplicitUserProfileContent.ParseAndValidateActiveProjectsJson(
                row.ActiveProjectsJson,
                nameof(ExplicitUserProfile.ActiveProjectsJson))
            .Select(p => new UserInterestProjectSnapshot(
                p.Name,
                string.IsNullOrWhiteSpace(p.ExternalId) ? null : p.ExternalId))
            .ToList();

        return new UserInterestSnapshot(
            row.CoreInterests,
            row.SecondaryInterests,
            row.Goals,
            projects);
    }
}
