using Platform.Domain.Features.Memory;

namespace Platform.Domain.Features.Memory.Entities;

/// <summary>Versioned workflow rules (see <c>procedural_rules</c>).</summary>
public sealed class ProceduralRule
{
    public long Id { get; set; }
    public int PrincipalId { get; set; }
    public string WorkflowType { get; set; } = "";
    public string RuleName { get; set; } = "";
    public string RuleContent { get; set; } = "";
    public int Priority { get; set; }
    public string Source { get; set; } = "";
    public int Version { get; set; }
    public ProceduralRuleStatus Status { get; set; }
}
