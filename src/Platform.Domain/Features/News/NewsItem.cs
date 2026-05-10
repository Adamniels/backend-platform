namespace Platform.Domain.Features.News;

public sealed class NewsItem
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Source { get; set; } = "";
    public DateTimeOffset PublishedAt { get; set; }

    public string Url { get; set; } = "";
    public string UrlHash { get; set; } = "";
    public string Body { get; set; } = "";
    public string? Author { get; set; }
    public string? SourceFeedUrl { get; set; }
}
