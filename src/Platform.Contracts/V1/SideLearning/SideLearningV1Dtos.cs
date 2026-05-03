using System.Text.Json.Serialization;

namespace Platform.Contracts.V1.SideLearning;

public sealed class CreateSideLearningSessionV1Request
{
    public string? InitialPrompt { get; set; }
}

public sealed record CreateSideLearningSessionV1Response(
    [property: JsonPropertyName("sessionId")] string SessionId,
    [property: JsonPropertyName("phase")] string Phase,
    [property: JsonPropertyName("workflowRunId")] string WorkflowRunId);

public sealed record SideLearningSessionSummaryV1Dto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("phase")] string Phase,
    [property: JsonPropertyName("createdAt")] string CreatedAt,
    [property: JsonPropertyName("updatedAt")] string UpdatedAt);

public sealed record SideLearningSessionV1Dto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("phase")] string Phase,
    [property: JsonPropertyName("initialPrompt")] string? InitialPrompt,
    [property: JsonPropertyName("selectedTopicTitle")] string? SelectedTopicTitle,
    [property: JsonPropertyName("selectedTopicReason")] string? SelectedTopicReason,
    [property: JsonPropertyName("topicProposalsJson")] string TopicProposalsJson,
    [property: JsonPropertyName("sessionContentJson")] string SessionContentJson,
    [property: JsonPropertyName("sectionsProgressJson")] string SectionsProgressJson,
    [property: JsonPropertyName("reflectionText")] string? ReflectionText,
    [property: JsonPropertyName("workflowRunId")] string? WorkflowRunId,
    [property: JsonPropertyName("createdAt")] string CreatedAt,
    [property: JsonPropertyName("updatedAt")] string UpdatedAt);

public sealed class SelectSideLearningTopicV1Request
{
    public string? TopicTitle { get; set; }

    public string? Feedback { get; set; }
}

public sealed class UpdateSideLearningProgressV1Request
{
    public string? SectionId { get; set; }

    public bool Completed { get; set; }
}

public sealed class SubmitSideLearningReflectionV1Request
{
    public string? Reflection { get; set; }
}
