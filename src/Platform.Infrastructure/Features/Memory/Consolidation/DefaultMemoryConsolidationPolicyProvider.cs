using Platform.Application.Abstractions.Memory.Consolidation;

namespace Platform.Infrastructure.Features.Memory.Consolidation;

public sealed class DefaultMemoryConsolidationPolicyProvider : IMemoryConsolidationPolicyProvider
{
    public int MinOccurrencesForPattern => 3;
    public double ReinforceConfidenceDelta => 0.06d;
    public double ProposalInitialConfidence => 0.42d;
    public int MaxEventsPerWindow => 10_000;
    public int ReviewQueuePriority => 2;
}
