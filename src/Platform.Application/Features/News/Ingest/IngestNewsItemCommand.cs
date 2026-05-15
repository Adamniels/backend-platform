namespace Platform.Application.Features.News.Ingest;

public sealed record IngestNewsItemCommand(
    string Title,
    string Url,
    string Source,
    string Body,
    string? Author,
    string PublishedAt,
    string? SourceFeedUrl);
