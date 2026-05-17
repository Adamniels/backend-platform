using System.Text.Json.Serialization;

namespace Platform.Contracts.V1.News;

public sealed record RecordNewsInteractionV1Request(
    [property: JsonPropertyName("newsItemId")] string NewsItemId,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("dwellSeconds")] int? DwellSeconds);
