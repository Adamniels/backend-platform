namespace Platform.Domain.Features.SavedItems;

public sealed class SavedItem
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Kind { get; set; } = "other";
    public DateTimeOffset SavedAt { get; set; }
}
