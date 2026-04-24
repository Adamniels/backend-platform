using System.Text.Json.Serialization;

namespace Platform.Contracts.Admin;

public sealed record UnlockRequest(
    [property: JsonPropertyName("accessKey")] string AccessKey);
