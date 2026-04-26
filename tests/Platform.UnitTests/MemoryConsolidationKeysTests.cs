using Platform.Application.Features.Memory.Consolidation;
using Xunit;

namespace Platform.UnitTests;

public sealed class MemoryConsolidationKeysTests
{
    [Fact]
    public void Semantic_key_is_stable_and_prefixed()
    {
        var k = MemoryConsolidationKeys.SemanticKeyFromEventType("integration.semantic");
        Assert.StartsWith("consolidation.event.", k, StringComparison.Ordinal);
        Assert.Contains("integration", k, StringComparison.Ordinal);
    }

    [Fact]
    public void Proposal_fingerprint_includes_user_window_and_hash()
    {
        var d = new DateOnly(2026, 4, 25);
        var fp = MemoryConsolidationKeys.ProposalFingerprint(1, d, "Foo.Bar");
        Assert.StartsWith("mcons_fp_v1:u1:d20260425:h", fp, StringComparison.Ordinal);
        Assert.True(fp.Length > 32);
    }
}
