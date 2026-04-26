namespace Platform.Contracts.V1.Memory;

public sealed class SemanticMemoryV1Dto
{
    public long Id { get; set; }
    public string Key { get; set; } = "";
    public string Claim { get; set; } = "";
    public string? Domain { get; set; }
    public double Confidence { get; set; }
    public double AuthorityWeight { get; set; }
    public string Status { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? LastSupportedAt { get; set; }
}

public sealed class CreateSemanticMemoryV1Request
{
    public int? UserId { get; set; }
    public string Key { get; set; } = "";
    public string Claim { get; set; } = "";
    public double Confidence { get; set; }
    public double? AuthorityWeight { get; set; }
    public string? Domain { get; set; }
    public string? Status { get; set; }
    public long EventId { get; set; }
    public double EvidenceStrength { get; set; }
    public string? EvidenceReason { get; set; }
}

public sealed class UpdateSemanticMemoryConfidenceV1Request
{
    public int? UserId { get; set; }
    public double Confidence { get; set; }
    public bool FromInferredSource { get; set; }
}

public sealed class AttachSemanticMemoryEvidenceV1Request
{
    public int? UserId { get; set; }
    public long EventId { get; set; }
    public double Strength { get; set; }
    public string? Reason { get; set; }
    public bool FromInferredSource { get; set; }
    public bool ReinforceConfidence { get; set; }
    public double ReinforceConfidenceDelta { get; set; }
    public DateTimeOffset? EventOccurredAt { get; set; }
}
