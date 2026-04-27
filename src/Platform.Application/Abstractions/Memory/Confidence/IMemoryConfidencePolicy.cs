using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Abstractions.Memory.Confidence;

public interface IMemoryConfidencePolicy
{
    SemanticConfidenceComputation Compute(
        SemanticMemory semantic,
        IReadOnlyList<SemanticEvidenceSignal> evidence,
        bool conflictsWithExplicitProfile,
        DateTimeOffset now);
}

public sealed record SemanticEvidenceSignal(
    MemoryEvidencePolarity Polarity,
    MemoryEvidenceSourceKind SourceKind,
    double Strength,
    double ReliabilityWeight,
    DateTimeOffset OccurredAt);

public sealed record SemanticConfidenceComputation(
    double Confidence,
    double SupportScore,
    double ContradictionScore,
    double RecencyScore,
    double SourceDiversityScore,
    double AuthorityBonus,
    double ExplicitConflictPenalty,
    bool HasMaterialContradiction);
