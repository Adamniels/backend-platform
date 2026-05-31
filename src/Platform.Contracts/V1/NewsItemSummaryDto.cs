namespace Platform.Contracts.V1;

public sealed record NewsItemSummaryDto(
    string Id,
    string Title,
    string Source,
    string PublishedAt,
    string? Url,
    string? Body,
    double? RelevanceScore,
    string? RelevanceExplanation = null);
