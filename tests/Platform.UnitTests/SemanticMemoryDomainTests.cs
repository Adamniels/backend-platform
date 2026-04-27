using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;
using Platform.Domain.Features.Memory.ValueObjects;
using Xunit;

namespace Platform.UnitTests;

public sealed class SemanticMemoryDomainTests
{
    [Fact]
    public void Reinforce_from_inferred_throws_when_authority_at_user_approval_floor()
    {
        var t = DateTimeOffset.UtcNow;
        var s = SemanticMemory.CreateInitial(
            1,
            "k",
            "c",
            0.5d,
            AuthorityWeight.UserApprovedSemantic,
            null,
            t);
        var ex = Assert.Throws<MemoryDomainException>(
            () => s.ReinforceWithEvidence(0.1d, t, t, fromInferredSource: true));
        Assert.Contains("Inferred", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Mark_rejected_only_from_pending_review()
    {
        var t = DateTimeOffset.UtcNow;
        var p = SemanticMemory.CreateInitial(
            1,
            "k",
            "c",
            0.5d,
            AuthorityWeight.Inferred,
            null,
            t,
            SemanticMemoryStatus.PendingReview);
        p.MarkRejected(t);
        Assert.Equal(SemanticMemoryStatus.Rejected, p.Status);

        var a = SemanticMemory.CreateInitial(1, "k2", "c", 0.5d, AuthorityWeight.Inferred, null, t);
        Assert.Throws<MemoryDomainException>(() => a.MarkRejected(t));
    }

    [Fact]
    public void Evidence_link_requires_valid_reliability_and_source()
    {
        var t = DateTimeOffset.UtcNow;
        Assert.Throws<MemoryDomainException>(
            () => MemoryEvidence.Link(
                1,
                2,
                3,
                0.5d,
                "reason",
                t,
                MemoryEvidencePolarity.Support,
                MemoryEvidenceSourceKind.Unspecified,
                0.5d));
        Assert.Throws<MemoryDomainException>(
            () => MemoryEvidence.Link(
                1,
                2,
                3,
                0.5d,
                "reason",
                t,
                MemoryEvidencePolarity.Support,
                MemoryEvidenceSourceKind.Workflow,
                1.5d));
    }
}
