using System.Text.Json.Serialization;

namespace Platform.Contracts.V1.News;

public sealed record StoreSummaryV1Request(
    [property: JsonPropertyName("summaryMarkdown")] string SummaryMarkdown);
