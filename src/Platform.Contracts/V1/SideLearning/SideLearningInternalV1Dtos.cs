using System.Text.Json;

namespace Platform.Contracts.V1.SideLearning;

public sealed class PostSideLearningTopicProposalsV1Request
{
    public IReadOnlyList<SideLearningTopicProposalV1Item>? Topics { get; set; }
}

public sealed class SideLearningTopicProposalV1Item
{
    public string? Title { get; set; }

    public string? Rationale { get; set; }

    public int? EstimatedMinutes { get; set; }

    public string? Difficulty { get; set; }

    public string? TargetSkillGap { get; set; }
}

public sealed class PostSideLearningSessionContentV1Request
{
    public JsonElement Sections { get; set; }

    public JsonElement? MemoryProposals { get; set; }
}

public sealed class PostSideLearningReflectionInsightsV1Request
{
    public IReadOnlyList<SideLearningMemoryProposalV1Item>? MemoryProposals { get; set; }
}

public sealed class SideLearningMemoryProposalV1Item
{
    public string? ProposalType { get; set; }

    public string? Title { get; set; }

    public string? Summary { get; set; }

    public string? ProposedChangeJson { get; set; }

    public string? EvidenceJson { get; set; }

    public int Priority { get; set; }
}
