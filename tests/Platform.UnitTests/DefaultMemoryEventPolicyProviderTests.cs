using Platform.Domain.Features.Memory;
using Platform.Infrastructure.Features.Memory.Events;

namespace Platform.UnitTests;

public sealed class DefaultMemoryEventPolicyProviderTests
{
    [Fact]
    public void Profile_events_are_reliable_but_not_auto_reinforced()
    {
        var policy = new DefaultMemoryEventPolicyProvider().Classify("profile.preference.updated");

        Assert.Equal(MemoryEventReliabilityClass.High, policy.ReliabilityClass);
        Assert.False(policy.AutoReinforceEligible);
        Assert.Equal(MemoryEvidenceSourceKind.UserAction, policy.DefaultSourceKind);
    }
}
