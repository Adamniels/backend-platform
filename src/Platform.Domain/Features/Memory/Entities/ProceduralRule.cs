using Platform.Domain.Features.Memory;

namespace Platform.Domain.Features.Memory.Entities;

public sealed class ProceduralRule
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public MemoryUser? User { get; set; }

    public string WorkflowType { get; set; } = "";
    public string RuleName { get; set; } = "";
    public string RuleContent { get; set; } = "";
    public int Priority { get; set; }
    public string Source { get; set; } = "";
    public int Version { get; set; }
    public ProceduralRuleStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public static ProceduralRule CreateFirstVersion(
        int userId,
        string workflowType,
        string ruleName,
        string ruleContent,
        int priority,
        string source,
        DateTimeOffset at)
    {
        if (string.IsNullOrWhiteSpace(workflowType) || string.IsNullOrWhiteSpace(ruleName))
        {
            throw new MemoryDomainException("WorkflowType and RuleName are required for procedural rules.");
        }

        if (string.IsNullOrWhiteSpace(source))
        {
            throw new MemoryDomainException("Source (provenance) is required for procedural rules.");
        }

        return new ProceduralRule
        {
            UserId = userId,
            WorkflowType = workflowType.Trim(),
            RuleName = ruleName.Trim(),
            RuleContent = ruleContent ?? "",
            Priority = priority,
            Source = source.Trim(),
            Version = 1,
            Status = ProceduralRuleStatus.Inactive,
            CreatedAt = at,
            UpdatedAt = at,
        };
    }

    public ProceduralRule NewVersionWithContent(string nextRuleContent, int nextVersion, DateTimeOffset at)
    {
        if (nextVersion != Version + 1)
        {
            throw new MemoryDomainException("Version must increase by 1 for a new procedural rule version.");
        }

        return new ProceduralRule
        {
            UserId = UserId,
            WorkflowType = WorkflowType,
            RuleName = RuleName,
            RuleContent = nextRuleContent ?? "",
            Priority = Priority,
            Source = Source,
            Version = nextVersion,
            Status = ProceduralRuleStatus.Inactive,
            CreatedAt = at,
            UpdatedAt = at,
        };
    }

    public void Activate(DateTimeOffset at)
    {
        Status = ProceduralRuleStatus.Active;
        UpdatedAt = at;
    }

    public void Deprecate(DateTimeOffset at)
    {
        Status = ProceduralRuleStatus.Deprecated;
        UpdatedAt = at;
    }
}
