namespace Platform.Application.Features.Memory.Procedural.PublishProceduralRuleVersion;

public sealed record PublishProceduralRuleVersionCommand(
    long BasisRuleId,
    int UserId,
    string RuleContent,
    double? AuthorityWeight,
    bool ForceSubmitForReview);
