namespace Platform.Application.Abstractions.News;

/// <summary>Ranks news articles by cosine similarity against the user's interest profile vector.</summary>
public interface INewsVectorSearch
{
    /// <summary>
    /// Returns up to <paramref name="limit"/> articles ordered by cosine similarity (highest first).
    /// Returns an empty list when no profile exists for the user — callers fall back to chronological.
    /// </summary>
    Task<IReadOnlyList<NewsVectorHit>> RankByRelevanceAsync(
        int userId,
        int limit,
        CancellationToken cancellationToken = default);
}

/// <summary>One article surfaced by vector similarity search.</summary>
public sealed record NewsVectorHit(
    string NewsItemId,
    double CosineSimilarity);   // 0–1, higher = more relevant
