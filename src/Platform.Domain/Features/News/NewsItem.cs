namespace Platform.Domain.Features.News;

public sealed class NewsItem
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Source { get; set; } = "";
    public DateTimeOffset PublishedAt { get; set; }
}
