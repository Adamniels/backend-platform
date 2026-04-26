using Platform.Infrastructure.Features.Memory.Consolidation;
using Xunit;

namespace Platform.UnitTests;

public sealed class DefaultMemoryConsolidationPolicyProviderTests
{
    private readonly DefaultMemoryConsolidationPolicyProvider _sut = new();

    [Theory]
    [InlineData("profile.updated", true)]
    [InlineData("PROFILE.goals", true)]
    [InlineData("explicit.memory", true)]
    [InlineData("preference.theme", true)]
    [InlineData("goal.ship", true)]
    [InlineData("identity.claim", true)]
    [InlineData("learning.session.completed", false)]
    [InlineData("workflow.step", false)]
    public void BlocksAutoReinforceForEventType_matches_prefix_policy(string eventType, bool expected)
    {
        Assert.Equal(expected, _sut.BlocksAutoReinforceForEventType(eventType));
    }

    [Fact]
    public void BlocksAutoReinforceForEventType_whitespace_blocks()
    {
        Assert.True(_sut.BlocksAutoReinforceForEventType("   "));
    }
}
