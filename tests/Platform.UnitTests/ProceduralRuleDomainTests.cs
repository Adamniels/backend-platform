using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.UnitTests;

public sealed class ProceduralRuleDomainTests
{
    [Fact]
    public void CreateFirstVersion_starts_inactive_with_version_one()
    {
        var t = DateTimeOffset.UtcNow;
        var r = ProceduralRule.CreateFirstVersion(1, "learning", "session_style", "be concise", 2, "user:settings", 0.9, t);
        Assert.Equal(1, r.Version);
        Assert.Equal(ProceduralRuleStatus.Inactive, r.Status);
        Assert.Equal(0.9d, r.AuthorityWeight);
    }

    [Fact]
    public void NewVersionWithContent_requires_monotonic_version()
    {
        var t = DateTimeOffset.UtcNow;
        var v1 = ProceduralRule.CreateFirstVersion(1, "w", "n", "a", 0, "s", 0.8, t);
        var v2 = v1.NewVersionWithContent("b", 2, t);
        Assert.Equal(2, v2.Version);
        Assert.Equal(0.8d, v2.AuthorityWeight);
        Assert.Throws<MemoryDomainException>(() => v1.NewVersionWithContent("x", 3, t));
    }

    [Fact]
    public void ShouldQueueReviewBeforeApply_respects_floor_and_force()
    {
        Assert.False(ProceduralRule.ShouldQueueReviewBeforeApply(0.78d, false));
        Assert.True(ProceduralRule.ShouldQueueReviewBeforeApply(0.77d, false));
        Assert.True(ProceduralRule.ShouldQueueReviewBeforeApply(1d, true));
    }
}
