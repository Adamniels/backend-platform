using Platform.Application.Abstractions.Memory.Confidence;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;
using Platform.Domain.Features.Memory.ValueObjects;

namespace Platform.Infrastructure.Features.Memory.Confidence;

public sealed class DefaultMemoryConfidencePolicy : IMemoryConfidencePolicy
{
    public SemanticConfidenceComputation Compute(
        SemanticMemory semantic,
        IReadOnlyList<SemanticEvidenceSignal> evidence,
        bool conflictsWithExplicitProfile,
        DateTimeOffset now)
    {
        var support = 0d;
        var contradiction = 0d;
        var recency = 0d;
        var sourceKinds = new HashSet<MemoryEvidenceSourceKind>();

        foreach (var e in evidence)
        {
            var polarityWeight = e.Polarity switch
            {
                MemoryEvidencePolarity.Support => 1d,
                MemoryEvidencePolarity.WeakSupport => 0.55d,
                MemoryEvidencePolarity.Contradict => -1d,
                MemoryEvidencePolarity.WeakContradict => -0.55d,
                MemoryEvidencePolarity.Supersede => -1.25d,
                _ => 0d,
            };
            var rec = RecencyScore(e.OccurredAt, now, halfLifeDays: 90d);
            var weighted = e.Strength * e.ReliabilityWeight * rec * Math.Abs(polarityWeight);
            if (polarityWeight >= 0)
            {
                support += weighted;
            }
            else
            {
                contradiction += weighted;
            }

            recency = Math.Max(recency, rec);
            sourceKinds.Add(e.SourceKind);
        }

        var diversity = evidence.Count == 0 ? 0d : Math.Clamp(sourceKinds.Count / 4d, 0d, 1d);
        var authorityBonus = semantic.AuthorityWeight >= AuthorityWeight.UserApprovedSemantic.Value
            ? 0.18d
            : semantic.AuthorityWeight >= AuthorityWeight.Inferred.Value
                ? 0.06d
                : 0d;
        var explicitPenalty = conflictsWithExplicitProfile ? 0.22d : 0d;
        var stalePenalty = semantic.LastSupportedAt is null
            ? 0.08d
            : (now - semantic.LastSupportedAt.Value).TotalDays > 120d ? 0.12d : 0d;

        var raw = 0.35d +
            0.32d * Saturate(support) -
            0.36d * Saturate(contradiction) +
            0.10d * recency +
            0.07d * diversity +
            authorityBonus -
            explicitPenalty -
            stalePenalty;
        var confidence = Math.Clamp(raw, 0.05d, 0.98d);
        var materialContradiction = contradiction >= 0.35d && contradiction >= support * 0.6d;

        return new SemanticConfidenceComputation(
            confidence,
            support,
            contradiction,
            recency,
            diversity,
            authorityBonus,
            explicitPenalty,
            materialContradiction);
    }

    private static double Saturate(double value) => 1d - Math.Exp(-Math.Max(0d, value));

    private static double RecencyScore(DateTimeOffset at, DateTimeOffset now, double halfLifeDays)
    {
        var days = Math.Max(0d, (now - at).TotalDays);
        return Math.Exp(-Math.Log(2d) * days / halfLifeDays);
    }
}
