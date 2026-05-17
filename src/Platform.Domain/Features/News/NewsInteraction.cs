namespace Platform.Domain.Features.News;

public enum NewsInteractionType
{
    Read,
    Save,
    Dismiss,
}

public sealed class NewsInteraction
{
    public long Id { get; set; }

    public int UserId { get; set; }

    /// <summary>FK to NewsItems.Id — restricted delete so events survive article removal.</summary>
    public string NewsItemId { get; set; } = "";

    public NewsInteractionType Type { get; set; }

    /// <summary>Only populated for Read interactions. Null for Save and Dismiss.</summary>
    public int? DwellSeconds { get; set; }

    public DateTimeOffset RecordedAt { get; set; }
}
