using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.UnitTests;

public sealed class MemoryReviewQueueItemDomainTests
{
    [Fact]
    public void Approve_and_reject_only_from_pending()
    {
        var t = DateTimeOffset.UtcNow;
        var row = MemoryReviewQueueItem.Propose(
            1,
            MemoryReviewProposalType.NewSemantic,
            "title",
            "summary",
            """{"kind":"NewSemantic","key":"a","claim":"b"}""",
            null,
            2,
            t);
        row.Approve(t.AddMinutes(1), 99L, null, "ok");
        Assert.Equal(MemoryReviewStatus.Approved, row.Status);
        Assert.Equal(99L, row.ApprovedSemanticMemoryId);
        Assert.Null(row.ApprovedProceduralRuleId);
        Assert.NotNull(row.ResolvedAt);

        var row2 = MemoryReviewQueueItem.Propose(
            1,
            MemoryReviewProposalType.NewSemantic,
            "x",
            "y",
            """{"kind":"NewSemantic","key":"c","claim":"d"}""",
            null,
            1,
            t);
        row2.Reject(t.AddMinutes(2), "no");
        Assert.Equal(MemoryReviewStatus.Rejected, row2.Status);
        Assert.Equal("no", row2.RejectedReason);

        Assert.Throws<MemoryDomainException>(() => row2.Reject(t, "again"));
    }

    [Fact]
    public void Pending_edits_update_fields()
    {
        var t = DateTimeOffset.UtcNow;
        var row = MemoryReviewQueueItem.Propose(
            1,
            MemoryReviewProposalType.NewSemantic,
            "old",
            "sum",
            "{}",
            null,
            0,
            t);
        row.ApplyPendingEdits("new", "s2", """{"kind":"NewSemantic","key":"k","claim":"x"}""", t.AddSeconds(1));
        Assert.Equal("new", row.Title);
        Assert.Equal("s2", row.Summary);
        Assert.Contains("NewSemantic", row.ProposedChangeJson ?? "", StringComparison.Ordinal);
    }
}
