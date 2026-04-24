using System.Text.Json.Serialization;

namespace Platform.Contracts.V1;

public sealed record StartWorkflowRunRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("workflowType")] string WorkflowType,
    [property: JsonPropertyName("taskQueue")] string? TaskQueue);