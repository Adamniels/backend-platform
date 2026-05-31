namespace Platform.Application.Features.News.RankedFeed;

public readonly record struct GetNewsFeedCandidatesQuery(int UserId, int Limit = 50);

public sealed record NewsFeedCandidatesResult(
    UserInterestContextDto           UserContext,
    IReadOnlyList<NewsFeedCandidateDto> Candidates);

/// <summary>Flattened user interest context for consumption by the LLM prompt builder in the worker.</summary>
public sealed record UserInterestContextDto(
    IReadOnlyList<string> CoreInterests,
    IReadOnlyList<string> SecondaryInterests,
    IReadOnlyList<string> Goals,
    IReadOnlyList<string> ActiveProjects);

/// <summary>One candidate article to pass to the LLM for re-ranking.</summary>
public sealed record NewsFeedCandidateDto(
    string  NewsItemId,
    string  Title,
    string  Source,
    string  PublishedAt,
    string? BodySnippet);
