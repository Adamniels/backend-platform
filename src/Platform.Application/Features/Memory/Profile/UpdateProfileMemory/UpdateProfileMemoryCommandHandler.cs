using FluentValidation;
using Platform.Application.Abstractions.Memory.Profile;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;
using Platform.Application.Features.Memory.Profile.GetProfileMemory;

namespace Platform.Application.Features.Memory.Profile.UpdateProfileMemory;

public sealed class UpdateProfileMemoryCommandHandler(
    IValidator<UpdateProfileMemoryCommand> validator,
    IExplicitUserProfileRepository profile,
    GetProfileMemoryQueryHandler getProfile)
{
    public async Task<ProfileMemoryV1Dto> HandleAsync(
        UpdateProfileMemoryCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken).ConfigureAwait(false);

        var userId = command.UserId is 0
            ? MemoryUser.DefaultId
            : command.UserId;

        var at = DateTimeOffset.UtcNow;
        var row = await profile
            .GetByUserIdAsync(userId, cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
        {
            row = new ExplicitUserProfile
            {
                UserId = userId,
                CreatedAt = at,
                AuthorityWeight = ExplicitUserProfileContent.ExplicitUserAuthorityValue,
            };
        }

        var prefs = command.Preferences
            .Select(p => new ProfileMemoryPreference(p.Key, p.Value))
            .ToList();
        var projects = command.ActiveProjects
            .Select(
                p => new ProfileMemoryProject(
                    p.Name,
                    string.IsNullOrWhiteSpace(p.ExternalId) ? null : p.ExternalId))
            .ToList();
        var skills = command.SkillLevels
            .Select(s => new ProfileMemorySkillLevel(s.Name, s.Level))
            .ToList();

        var pJson = ExplicitUserProfileContent.SerialisePreferencesJson(prefs);
        var projJson = ExplicitUserProfileContent.SerialiseProjectsJson(projects);
        var sJson = ExplicitUserProfileContent.SerialiseSkillLevelsJson(skills);

        row.ApplyUserUpdate(
            command.CoreInterests,
            command.SecondaryInterests,
            command.Goals,
            pJson,
            projJson,
            sJson,
            at);

        _ = await profile
            .SaveAsync(row, cancellationToken)
            .ConfigureAwait(false);

        return await getProfile
            .HandleAsync(new GetProfileMemoryQuery(userId), cancellationToken)
            .ConfigureAwait(false);
    }
}
