namespace Platform.Contracts.V1.Memory;

/// <summary>Curated memory packet for agents/workflows (v1: SQL + in-memory rank; no vectors/graph).</summary>
public sealed class MemoryContextV1Dto
{
    public IReadOnlyList<ProfileFactV1Dto> ProfileFacts { get; set; } = Array.Empty<ProfileFactV1Dto>();
    public IReadOnlyList<ActiveGoalV1Dto> ActiveGoals { get; set; } = Array.Empty<ActiveGoalV1Dto>();
    public IReadOnlyList<RelevantProjectV1Dto> RelevantProjects { get; set; } = Array.Empty<RelevantProjectV1Dto>();
    public IReadOnlyList<SemanticMemoryContextV1Dto> SemanticMemories { get; set; } = Array.Empty<SemanticMemoryContextV1Dto>();
    public IReadOnlyList<EpisodicExampleV1Dto> EpisodicExamples { get; set; } = Array.Empty<EpisodicExampleV1Dto>();
    public IReadOnlyList<ProceduralRuleContextV1Dto> ProceduralRules { get; set; } = Array.Empty<ProceduralRuleContextV1Dto>();
    public IReadOnlyList<MemoryConflictV1Dto> Conflicts { get; set; } = Array.Empty<MemoryConflictV1Dto>();
    public IReadOnlyList<MemoryWarningV1Dto> Warnings { get; set; } = Array.Empty<MemoryWarningV1Dto>();
    public string AssemblyStage { get; set; } = "v1-sql";
}

public sealed class GetMemoryContextV1Request
{
    public int? UserId { get; set; }
    public string? TaskDescription { get; set; }
    public string? WorkflowType { get; set; }
    public string? ProjectId { get; set; }
    public string? Domain { get; set; }
}

public sealed class ProfileFactV1Dto
{
    public string Source { get; set; } = "";
    public string Text { get; set; } = "";
    public double AuthorityWeight { get; set; }
    public double RankScore { get; set; }
}

public sealed class ActiveGoalV1Dto
{
    public string Goal { get; set; } = "";
    public double AuthorityWeight { get; set; }
    public double RankScore { get; set; }
}

public sealed class RelevantProjectV1Dto
{
    public string Name { get; set; } = "";
    public string? ExternalId { get; set; }
    public double RankScore { get; set; }
}

public sealed class SemanticMemoryContextV1Dto
{
    public long Id { get; set; }
    public string Key { get; set; } = "";
    public string Claim { get; set; } = "";
    public string? Domain { get; set; }
    public double Confidence { get; set; }
    public double AuthorityWeight { get; set; }
    public string Status { get; set; } = "";
    public DateTimeOffset UpdatedAt { get; set; }
    public double RankScore { get; set; }
}

public sealed class EpisodicExampleV1Dto
{
    public long Id { get; set; }
    public string EventType { get; set; } = "";
    public string? Domain { get; set; }
    public string? WorkflowId { get; set; }
    public string? ProjectId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string? PayloadPreview { get; set; }
    public double RankScore { get; set; }
}

public sealed class ProceduralRuleContextV1Dto
{
    public long Id { get; set; }
    public string WorkflowType { get; set; } = "";
    public string RuleName { get; set; } = "";
    public string RuleContent { get; set; } = "";
    public int Priority { get; set; }
    public int Version { get; set; }
    public string Status { get; set; } = "";
    public double RankScore { get; set; }
}

public sealed class MemoryConflictV1Dto
{
    public string Kind { get; set; } = "";
    public string Summary { get; set; } = "";
    public IReadOnlyList<string> RelatedEntityIds { get; set; } = Array.Empty<string>();
}

public sealed class MemoryWarningV1Dto
{
    public string Code { get; set; } = "";
    public string Message { get; set; } = "";
}
