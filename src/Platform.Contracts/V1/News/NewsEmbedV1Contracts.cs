using System.Text.Json.Serialization;

namespace Platform.Contracts.V1.News;

public sealed record EmbedNewsItemV1Response(
    [property: JsonPropertyName("status")] string Status);

public sealed record SeedNewsProfileV1Request(
    [property: JsonPropertyName("userId")] int UserId);

public sealed record SeedNewsProfileV1Response(
    [property: JsonPropertyName("status")] string Status);
