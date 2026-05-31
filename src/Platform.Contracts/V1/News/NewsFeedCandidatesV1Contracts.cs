using System.Text.Json.Serialization;

namespace Platform.Contracts.V1.News;

// ── Candidates endpoint (GET feed/candidates) ────────────────────────────────

public sealed record StoreRankedResultsV1Request(
    [property: JsonPropertyName("userId")]    int    UserId,
    [property: JsonPropertyName("modelUsed")] string ModelUsed,
    [property: JsonPropertyName("rankings")]  IReadOnlyList<RankedResultEntryV1> Rankings);

public sealed record RankedResultEntryV1(
    [property: JsonPropertyName("newsItemId")]   string NewsItemId,
    [property: JsonPropertyName("score")]        int    Score,
    [property: JsonPropertyName("explanation")]  string Explanation);
