using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.ValueObjects;

namespace Platform.Domain.Features.Memory.Entities;

/// <summary>Learned claim with confidence (see <c>semantic_memories</c>).</summary>
public sealed class SemanticMemory
{
    public long Id { get; set; }
    public int PrincipalId { get; set; }
    public string Key { get; set; } = "";
    public string Claim { get; set; } = "";
    public string? Domain { get; set; }
    public double Confidence { get; set; }
    public AuthorityWeight AuthorityWeight { get; set; }
    public MemoryItemStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? LastSupportedAt { get; set; }
}
