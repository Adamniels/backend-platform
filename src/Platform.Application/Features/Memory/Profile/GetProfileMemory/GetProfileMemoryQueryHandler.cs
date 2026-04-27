using Platform.Application.Abstractions.Memory.Profile;
using Platform.Application.Abstractions.Memory.Users;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Features.Memory.Profile.GetProfileMemory;

public sealed class GetProfileMemoryQueryHandler(
    IExplicitUserProfileRepository profile,
    IMemoryUserContextResolver userResolver)
{
    public async Task<ProfileMemoryV1Dto> HandleAsync(
        GetProfileMemoryQuery query,
        CancellationToken cancellationToken = default)
    {
        var id = userResolver.Resolve(query.UserId);

        var row = await profile
            .GetByUserIdAsync(id, cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
        {
            return new ProfileMemoryV1Dto
            {
                AuthorityWeight = ExplicitUserProfileContent.ExplicitUserAuthorityValue,
            };
        }

        var parsedPrefs = ExplicitUserProfileContent.ParseAndValidatePreferencesJson(
            row.PreferencesJson,
            nameof(ExplicitUserProfile.PreferencesJson));
        var parsedProjects = ExplicitUserProfileContent.ParseAndValidateActiveProjectsJson(
            row.ActiveProjectsJson,
            nameof(ExplicitUserProfile.ActiveProjectsJson));
        var parsedSkills = ExplicitUserProfileContent.ParseAndValidateSkillLevelsJson(
            row.SkillLevelsJson,
            nameof(ExplicitUserProfile.SkillLevelsJson));

        return new ProfileMemoryV1Dto
        {
            Id = row.Id,
            CoreInterests = row.CoreInterests,
            SecondaryInterests = row.SecondaryInterests,
            Goals = row.Goals,
            Preferences = parsedPrefs
                .Select(
                    p => new ProfileMemoryPreferenceV1
                    {
                        Key = p.Key,
                        Value = p.Value,
                    })
                .ToList(),
            ActiveProjects = parsedProjects
                .Select(
                    p => new ProfileMemoryProjectV1
                    {
                        Name = p.Name,
                        ExternalId = string.IsNullOrWhiteSpace(p.ExternalId) ? null : p.ExternalId,
                    })
                .ToList(),
            SkillLevels = parsedSkills
                .Select(
                    s => new ProfileMemorySkillLevelV1
                    {
                        Name = s.Name,
                        Level = s.Level,
                    })
                .ToList(),
            AuthorityWeight = row.AuthorityWeight,
            CreatedAt = row.CreatedAt,
            UpdatedAt = row.UpdatedAt,
        };
    }
}
