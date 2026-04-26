namespace Platform.Contracts.V1.Memory;

public sealed class ProfileMemoryV1Dto
{
    public long? Id { get; set; }
    public IReadOnlyList<string> CoreInterests { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> SecondaryInterests { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> Goals { get; set; } = Array.Empty<string>();
    public IReadOnlyList<ProfileMemoryPreferenceV1> Preferences { get; set; } = Array.Empty<ProfileMemoryPreferenceV1>();
    public IReadOnlyList<ProfileMemoryProjectV1> ActiveProjects { get; set; } = Array.Empty<ProfileMemoryProjectV1>();
    public IReadOnlyList<ProfileMemorySkillLevelV1> SkillLevels { get; set; } = Array.Empty<ProfileMemorySkillLevelV1>();
    public double AuthorityWeight { get; set; } = 1.0d;
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class UpdateProfileMemoryV1Request
{
    public IReadOnlyList<string> CoreInterests { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> SecondaryInterests { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> Goals { get; set; } = Array.Empty<string>();
    public IReadOnlyList<ProfileMemoryPreferenceV1> Preferences { get; set; } = Array.Empty<ProfileMemoryPreferenceV1>();
    public IReadOnlyList<ProfileMemoryProjectV1> ActiveProjects { get; set; } = Array.Empty<ProfileMemoryProjectV1>();
    public IReadOnlyList<ProfileMemorySkillLevelV1> SkillLevels { get; set; } = Array.Empty<ProfileMemorySkillLevelV1>();
}

public sealed class ProfileMemoryPreferenceV1
{
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
}

public sealed class ProfileMemoryProjectV1
{
    public string Name { get; set; } = "";
    public string? ExternalId { get; set; }
}

public sealed class ProfileMemorySkillLevelV1
{
    public string Name { get; set; } = "";
    public double Level { get; set; }
}
