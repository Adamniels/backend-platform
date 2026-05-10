using System.Text.Json.Serialization;

namespace Platform.Contracts.V1.News;

public sealed record DeleteNewsItemsV1Request(
    [property: JsonPropertyName("ids")] string[] Ids);

public sealed record DeleteNewsItemsV1Response(
    [property: JsonPropertyName("deleted")] int Deleted);
