namespace Platform.Contracts.V1.Memory;

public sealed class CreateProceduralRuleV1Request
{
    public int? UserId { get; set; }
    public string WorkflowType { get; set; } = "";
    public string RuleName { get; set; } = "";
    public string RuleContent { get; set; } = "";
    public int Priority { get; set; }
    public string Source { get; set; } = "";
    public double AuthorityWeight { get; set; } = 0.92d;
    public bool ForceSubmitForReview { get; set; }
}

public sealed class CreateProceduralRuleV1Response
{
    public string Outcome { get; set; } = "";
    public long? RuleId { get; set; }
    public long? ReviewQueueItemId { get; set; }
}

public sealed class PublishProceduralRuleVersionV1Request
{
    public int? UserId { get; set; }
    public string RuleContent { get; set; } = "";
    public double? AuthorityWeight { get; set; }
    public bool ForceSubmitForReview { get; set; }
}

public sealed class PublishProceduralRuleVersionV1Response
{
    public string Outcome { get; set; } = "";
    public long? RuleId { get; set; }
    public long? ReviewQueueItemId { get; set; }
}

public sealed class UpdateProceduralRulePriorityV1Request
{
    public int? UserId { get; set; }
    public int Priority { get; set; }
}

public sealed record ProceduralRuleDetailV1Dto(
    long Id,
    int UserId,
    string WorkflowType,
    string RuleName,
    string RuleContent,
    int Priority,
    string Source,
    double AuthorityWeight,
    int Version,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

/// <summary>Payload for proposal type <c>NewProceduralRule</c> in <c>ProposedChangeJson</c>.</summary>
public sealed class NewProceduralRuleMemoryProposalV1
{
    public string? WorkflowType { get; set; }
    public string? RuleName { get; set; }
    public string RuleContent { get; set; } = "";
    public int Priority { get; set; }
    public string Source { get; set; } = "";
    public double AuthorityWeight { get; set; }
    public long? BasisRuleId { get; set; }
}
