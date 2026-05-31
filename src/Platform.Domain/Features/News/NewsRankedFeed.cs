namespace Platform.Domain.Features.News;

/// <summary>
/// Pre-computed LLM-ranked feed for a user. One row per user, fully replaced on each ingestion run.
/// EntriesJson holds an ordered JSON array of { newsItemId, score, explanation } objects.
/// </summary>
public sealed class NewsRankedFeed
{
    /// <summary>PK — one ranked feed per user.</summary>
    public int UserId { get; set; }

    /// <summary>
    /// Ordered JSON array of ranked entries, serialized as JSONB.
    /// Each entry: { "newsItemId": "...", "score": 92, "explanation": "..." }
    /// </summary>
    public string EntriesJson { get; set; } = "[]";

    /// <summary>The Anthropic model string that produced this ranking.</summary>
    public string ModelUsed { get; set; } = "";

    public DateTimeOffset RankedAt { get; set; }
}
