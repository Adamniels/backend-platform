using Pgvector;

namespace Platform.Domain.Features.News;

public sealed class NewsUserProfile
{
    /// <summary>PK — one profile per user.</summary>
    public int UserId { get; set; }

    /// <summary>1536-dim long-term interest embedding seeded from IUserInterestProvider.</summary>
    public Vector LongTermEmbedding { get; set; } = null!;

    /// <summary>The text that was embedded — stored for debugging and re-seed detection.</summary>
    public string SeedText { get; set; } = "";

    public DateTimeOffset SeededAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
