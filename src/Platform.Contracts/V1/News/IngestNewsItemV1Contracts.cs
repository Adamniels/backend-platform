using System.Text.Json.Serialization;

namespace Platform.Contracts.V1.News;

public sealed record IngestNewsItemV1Request(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("body")] string Body,
    [property: JsonPropertyName("author")] string? Author,
    [property: JsonPropertyName("publishedAt")] string PublishedAt,
    [property: JsonPropertyName("sourceFeedUrl")] string? SourceFeedUrl);

public sealed record IngestNewsItemV1Response(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("id")] string? Id);

/// <summary>Optional display name for the workflow run row.</summary>
public sealed record TriggerNewsIntelligenceWorkflowV1Request(
    [property: JsonPropertyName("name")] string? Name);
