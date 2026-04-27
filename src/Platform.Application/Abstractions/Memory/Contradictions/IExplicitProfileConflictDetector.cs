using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Abstractions.Memory.Contradictions;

public interface IExplicitProfileConflictDetector
{
    IReadOnlyList<ExplicitProfileSemanticConflict> Detect(
        ExplicitUserProfile? profile,
        IReadOnlyList<SemanticMemory> semantics);
}

public sealed record ExplicitProfileSemanticConflict(
    long SemanticMemoryId,
    string Kind,
    string ExplicitText,
    string Claim,
    double Confidence,
    double AuthorityWeight);
