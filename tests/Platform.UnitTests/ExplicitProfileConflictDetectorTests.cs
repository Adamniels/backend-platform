using Platform.Domain.Features.Memory.Entities;
using Platform.Domain.Features.Memory.ValueObjects;
using Platform.Infrastructure.Features.Memory.Contradictions;

namespace Platform.UnitTests;

public sealed class ExplicitProfileConflictDetectorTests
{
    [Fact]
    public void Detects_negative_semantic_against_explicit_interest()
    {
        var now = DateTimeOffset.UtcNow;
        var profile = new ExplicitUserProfile
        {
            UserId = 1,
            CoreInterests = ["backend architecture"],
            PreferencesJson = "[]",
            ActiveProjectsJson = "[]",
            SkillLevelsJson = "[]",
        };
        var semantic = SemanticMemory.CreateInitial(
            1,
            "interest.backend",
            "User is no longer interested in backend architecture.",
            0.6,
            AuthorityWeight.Inferred,
            "learning",
            now);

        var conflicts = new ExplicitProfileConflictDetector().Detect(profile, [semantic]);

        Assert.Single(conflicts);
        Assert.Equal(semantic.Id, conflicts[0].SemanticMemoryId);
    }
}
