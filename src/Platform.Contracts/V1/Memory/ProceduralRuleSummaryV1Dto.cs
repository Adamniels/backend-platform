namespace Platform.Contracts.V1.Memory;

public sealed record ProceduralRuleSummaryV1Dto(
    long Id,
    string WorkflowType,
    string RuleName,
    int Version,
    int Priority,
    string Status,
    double AuthorityWeight,
    string Source,
    DateTimeOffset UpdatedAt);
