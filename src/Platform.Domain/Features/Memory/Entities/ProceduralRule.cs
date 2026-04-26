using Platform.Domain.Features.Memory;
using MemoryAuthority = Platform.Domain.Features.Memory.ValueObjects.AuthorityWeight;

namespace Platform.Domain.Features.Memory.Entities;

public sealed class ProceduralRule
{
    /// <summary>Below this authority (0.0–1.0), new rules and content versions should go through the review queue unless the caller forces direct apply.</summary>
    public const double ReviewAuthorityFloorForDirectApply = 0.78d;

    public long Id { get; set; }
    public int UserId { get; set; }
    public MemoryUser? User { get; set; }

    public string WorkflowType { get; set; } = "";
    public string RuleName { get; set; } = "";
    public string RuleContent { get; set; } = "";
    public int Priority { get; set; }
    public string Source { get; set; } = "";
    /// <summary>0.0–1.0; influences ranking in <c>GetMemoryContext</c> and whether a change is review-first.</summary>
    public double AuthorityWeight { get; set; }
    public int Version { get; set; }
    public ProceduralRuleStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public static bool ShouldQueueReviewBeforeApply(double authorityWeight, bool forceReview) =>
        forceReview || authorityWeight < ReviewAuthorityFloorForDirectApply;

    public static ProceduralRule CreateFirstVersion(
        int userId,
        string workflowType,
        string ruleName,
        string ruleContent,
        int priority,
        string source,
        double authorityWeight,
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

        if (!MemoryAuthority.TryCreate(authorityWeight, out var auth))
        {
            throw new MemoryDomainException("Authority weight is out of range.");
        }

        return new ProceduralRule
        {
            UserId = userId,
            WorkflowType = workflowType.Trim(),
            RuleName = ruleName.Trim(),
            RuleContent = ruleContent ?? "",
            Priority = priority,
            Source = source.Trim(),
            AuthorityWeight = auth.Value,
            Version = 1,
            Status = ProceduralRuleStatus.Inactive,
            CreatedAt = at,
            UpdatedAt = at,
        };
    }

    public ProceduralRule NewVersionWithContent(
        string nextRuleContent,
        int nextVersion,
        DateTimeOffset at,
        double? nextAuthorityWeight = null)
    {
        if (nextVersion != Version + 1)
        {
            throw new MemoryDomainException("Version must increase by 1 for a new procedural rule version.");
        }

        var nextAuth = nextAuthorityWeight is null
            ? this.AuthorityWeight
            : MemoryAuthority.TryCreate(nextAuthorityWeight.Value, out var na)
                ? na.Value
                : throw new MemoryDomainException("Authority weight is out of range.");

        return new ProceduralRule
        {
            UserId = UserId,
            WorkflowType = WorkflowType,
            RuleName = RuleName,
            RuleContent = nextRuleContent ?? "",
            Priority = Priority,
            Source = Source,
            AuthorityWeight = nextAuth,
            Version = nextVersion,
            Status = ProceduralRuleStatus.Inactive,
            CreatedAt = at,
            UpdatedAt = at,
        };
    }

    public void SetPriority(int priority, DateTimeOffset at)
    {
        if (priority < 0)
        {
            throw new MemoryDomainException("Priority must be non-negative.");
        }

        Priority = priority;
        UpdatedAt = at;
    }

    public void SetAuthorityWeight(double authorityWeight, DateTimeOffset at)
    {
        if (!MemoryAuthority.TryCreate(authorityWeight, out var auth))
        {
            throw new MemoryDomainException("Authority weight is out of range.");
        }

        this.AuthorityWeight = auth.Value;
        UpdatedAt = at;
    }

    public void SetProvenance(string source, DateTimeOffset at)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return;
        }

        Source = source.Trim();
        UpdatedAt = at;
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
