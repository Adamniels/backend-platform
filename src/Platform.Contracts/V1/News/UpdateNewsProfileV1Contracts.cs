using System.Text.Json.Serialization;

namespace Platform.Contracts.V1.News;

public sealed record UpdateNewsProfileV1Request(
    [property: JsonPropertyName("userId")] int UserId,
    [property: JsonPropertyName("windowDays")] int? WindowDays);

public sealed record UpdateNewsProfileV1Response(
    [property: JsonPropertyName("status")] string Status);
