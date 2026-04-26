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
}
