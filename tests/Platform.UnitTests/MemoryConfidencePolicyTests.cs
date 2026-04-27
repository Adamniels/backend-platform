using Platform.Application.Abstractions.Memory.Confidence;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;
using Platform.Domain.Features.Memory.ValueObjects;
using Platform.Infrastructure.Features.Memory.Confidence;

namespace Platform.UnitTests;

public sealed class MemoryConfidencePolicyTests
{
    [Fact]
    public void Contradicting_evidence_lowers_confidence()
    {
        var now = DateTimeOffset.UtcNow;
        var semantic = SemanticMemory.CreateInitial(
            1,
            "preference.depth",
            "User prefers deep explanations.",
            0.7,
            AuthorityWeight.Inferred,
            "learning",
            now.AddDays(-10));
        var policy = new DefaultMemoryConfidencePolicy();

        var supportOnly = policy.Compute(
            semantic,
            [
                new SemanticEvidenceSignal(
                    MemoryEvidencePolarity.Support,
                    MemoryEvidenceSourceKind.Workflow,
                    0.8,
                    0.8,
                    now.AddDays(-1)),
            ],
            conflictsWithExplicitProfile: false,
            now);
        var withContradiction = policy.Compute(
            semantic,
            [
                new SemanticEvidenceSignal(
                    MemoryEvidencePolarity.Support,
                    MemoryEvidenceSourceKind.Workflow,
                    0.8,
                    0.8,
                    now.AddDays(-1)),
                new SemanticEvidenceSignal(
                    MemoryEvidencePolarity.Contradict,
                    MemoryEvidenceSourceKind.UserAction,
                    0.9,
                    0.92,
                    now),
            ],
            conflictsWithExplicitProfile: false,
            now);

        Assert.True(withContradiction.Confidence < supportOnly.Confidence);
        Assert.True(withContradiction.HasMaterialContradiction);
    }
}
