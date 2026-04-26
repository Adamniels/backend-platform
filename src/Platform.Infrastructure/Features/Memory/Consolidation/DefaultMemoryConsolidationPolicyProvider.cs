using Platform.Application.Abstractions.Memory.Consolidation;

namespace Platform.Infrastructure.Features.Memory.Consolidation;

public sealed class DefaultMemoryConsolidationPolicyProvider : IMemoryConsolidationPolicyProvider
{
    /// <summary>Event-type prefixes that must not drive automated confidence reinforcement.</summary>
    private static readonly string[] AutoReinforceBlockedPrefixes =
    [
        "profile.",
        "explicit.",
        "preference.",
        "goal.",
        "identity.",
    ];

    public int MinOccurrencesForPattern => 3;
    public double ReinforceConfidenceDelta => 0.06d;
    public double ProposalInitialConfidence => 0.42d;
    public int MaxEventsPerWindow => 10_000;
    public int ReviewQueuePriority => 2;

    public bool BlocksAutoReinforceForEventType(string eventType)
    {
        var t = eventType.Trim();
        if (t.Length == 0)
        {
            return true;
        }

        foreach (var prefix in AutoReinforceBlockedPrefixes)
        {
            if (t.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
