using Platform.Contracts.V1.Memory;

namespace Platform.Application.Features.Memory.Profile.UpdateProfileMemory;

public sealed record UpdateProfileMemoryCommand(
    int UserId,
    IReadOnlyList<string> CoreInterests,
    IReadOnlyList<string> SecondaryInterests,
    IReadOnlyList<string> Goals,
    IReadOnlyList<ProfileMemoryPreferenceV1> Preferences,
    IReadOnlyList<ProfileMemoryProjectV1> ActiveProjects,
    IReadOnlyList<ProfileMemorySkillLevelV1> SkillLevels);
