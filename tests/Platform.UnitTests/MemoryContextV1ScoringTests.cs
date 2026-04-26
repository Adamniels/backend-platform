using Platform.Application.Features.Memory.Context;

namespace Platform.UnitTests;

public sealed class MemoryContextV1ScoringTests
{
    [Fact]
    public void Recency_is_higher_for_newer_timestamps()
    {
        var now = new DateTimeOffset(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);
        var old = now.AddDays(-60);
        var recent = now.AddDays(-2);
        var sOld = MemoryContextV1Scoring.RecencyScore(old, now, 30d);
        var sNew = MemoryContextV1Scoring.RecencyScore(recent, now, 30d);
        Assert.True(sNew > sOld);
    }

    [Fact]
    public void Explicit_profile_rank_floor_pulls_low_raw_scores_above_typical_inferred_semantic_ranks()
    {
        var explicitRank = MemoryContextV1Scoring.ExplicitProfileItemRank(0.25d);
        var inferredSemantic = MemoryContextV1Scoring.CombinedLearnerRank(
            authority: 0.55d,
            confidence: 0.6d,
            recency: 0.5d,
            workflowRel: 0.4d,
            projectRel: 0.35d,
            textMatch: 0.5d,
            domainMatch: 0.5d,
            statusFactor: 1d);
        Assert.True(explicitRank >= MemoryContextV1Scoring.ExplicitProfileRankFloor);
        Assert.True(explicitRank > inferredSemantic);
    }

    [Fact]
    public void Tokenize_finds_words()
    {
        var t = MemoryContextV1Scoring.Tokenize("Build a JWT learning session");
        Assert.Contains("jwt", t, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("learning", t, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void TextMatchRatio_is_higher_when_query_tokens_hit_fields()
    {
        var q = MemoryContextV1Scoring.Tokenize("temporal workflow");
        var low = MemoryContextV1Scoring.TextMatchRatio(q, "unrelated content about cooking");
        var high = MemoryContextV1Scoring.TextMatchRatio(q, "We use temporal for this workflow");
        Assert.True(high > low);
    }

    [Fact]
    public void WorkflowRelevance_matches_equal_strings()
    {
        var a = MemoryContextV1Scoring.WorkflowRelevance("learning", "learning");
        var b = MemoryContextV1Scoring.WorkflowRelevance("learning", "digest");
        Assert.True(a > b);
    }
}
