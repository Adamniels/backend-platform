using Platform.Application.Features.Memory.Profile.UpdateProfileMemory;
using Platform.Contracts.V1.Memory;

namespace Platform.UnitTests;

public sealed class UpdateProfileMemoryCommandValidatorTests
{
    [Fact]
    public void Skill_level_outside_0_1_fails()
    {
        var v = new UpdateProfileMemoryCommandValidator();
        var res = v.Validate(
            new UpdateProfileMemoryCommand(
                0,
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<ProfileMemoryPreferenceV1>(),
                Array.Empty<ProfileMemoryProjectV1>(),
                [new ProfileMemorySkillLevelV1 { Name = "x", Level = 1.1 }]));
        Assert.False(res.IsValid);
    }

    [Fact]
    public void Too_many_core_interests_fails()
    {
        var v = new UpdateProfileMemoryCommandValidator();
        var many = Enumerable.Repeat("a", 65).ToList();
        var res = v.Validate(
            new UpdateProfileMemoryCommand(
                0,
                many,
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<ProfileMemoryPreferenceV1>(),
                Array.Empty<ProfileMemoryProjectV1>(),
                Array.Empty<ProfileMemorySkillLevelV1>()));
        Assert.False(res.IsValid);
    }

    [Fact]
    public void Valid_minimal_update_passes()
    {
        var v = new UpdateProfileMemoryCommandValidator();
        var res = v.Validate(
            new UpdateProfileMemoryCommand(
                0,
                new[] { "ai" },
                new[] { "history" },
                new[] { "ship v1" },
                new[] { new ProfileMemoryPreferenceV1 { Key = "tz", Value = "utc" } },
                new[] { new ProfileMemoryProjectV1 { Name = "Platform" } },
                new[] { new ProfileMemorySkillLevelV1 { Name = "csharp", Level = 0.7 } }));
        Assert.True(res.IsValid);
    }
}
