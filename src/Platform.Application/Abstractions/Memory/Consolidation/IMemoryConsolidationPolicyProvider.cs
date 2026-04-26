namespace Platform.Application.Abstractions.Memory.Consolidation;

/// <summary>Thresholds for nightly consolidation; defaults are code-backed, swappable for DB-driven policy later.</summary>
public interface IMemoryConsolidationPolicyProvider
{
    /// <summary>Minimum identical <c>EventType</c> occurrences in the window to form a pattern.</summary>
    int MinOccurrencesForPattern { get; }

    /// <summary>Confidence delta applied when reinforcing an existing semantic from a pattern (clamped by domain rules).</summary>
    double ReinforceConfidenceDelta { get; }

    /// <summary>Initial confidence for <see cref="Domain.Features.Memory.MemoryReviewProposalType.NewSemantic"/> proposals.</summary>
    double ProposalInitialConfidence { get; }

    /// <summary>Hard cap on events read from the store for one window (safety).</summary>
    int MaxEventsPerWindow { get; }

    /// <summary>Priority for consolidation-created review items (lower = less urgent unless product inverts).</summary>
    int ReviewQueuePriority { get; }
}
