namespace Platform.Application.Features.Memory.Procedural.CreateProceduralRule;

public sealed record CreateProceduralRuleCommand(
    int UserId,
    string WorkflowType,
    string RuleName,
    string RuleContent,
    int Priority,
    string Source,
    double AuthorityWeight,
    bool ForceSubmitForReview);
