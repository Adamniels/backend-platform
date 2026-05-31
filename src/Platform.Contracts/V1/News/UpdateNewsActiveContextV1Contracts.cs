using System.Text.Json.Serialization;

namespace Platform.Contracts.V1.News;

public sealed record UpdateNewsActiveContextV1Request(
    [property: JsonPropertyName("userId")] int UserId);

public sealed record UpdateNewsActiveContextV1Response(
    [property: JsonPropertyName("status")] string Status);
