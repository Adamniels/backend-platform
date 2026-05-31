namespace Platform.Application.Features.News.RankedFeed;

/// <summary>A single entry in the LLM-ranked feed.</summary>
public sealed record RankedFeedEntry(
    string NewsItemId,
    int    Score,
    string Explanation);

public sealed record StoreRankedNewsFeedCommand(
    int                            UserId,
    string                         ModelUsed,
    IReadOnlyList<RankedFeedEntry> Rankings);

public enum StoreRankedNewsFeedResult
{
    Stored,
    Error,
}
