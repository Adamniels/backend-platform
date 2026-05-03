namespace Platform.Domain.Features.SideLearning;

public sealed class SideLearningSession
{
    public string Id { get; set; } = "";

    public int UserId { get; set; }

    public SideLearningSessionPhase Phase { get; set; }

    public string? InitialPrompt { get; set; }

    public string? SelectedTopicTitle { get; set; }

    public string? SelectedTopicReason { get; set; }

    /// <summary>JSON array of topic proposals from the worker (see side-learning plan).</summary>
    public string TopicProposalsJson { get; set; } = "[]";

    /// <summary>JSON object with session sections from the worker.</summary>
    public string SessionContentJson { get; set; } = "{}";

    /// <summary>JSON object mapping section id to completed flag.</summary>
    public string SectionsProgressJson { get; set; } = "{}";

    public string? ReflectionText { get; set; }

    public string? WorkflowRunId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
