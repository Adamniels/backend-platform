namespace Platform.Contracts.V1.Memory;

public sealed class CreateMemoryReviewQueueItemV1Request
{
    public int? UserId { get; set; }
    public string ProposalType { get; set; } = "";
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
    public string? ProposedChangeJson { get; set; }
    public string? EvidenceJson { get; set; }
    public int Priority { get; set; }
}

/// <summary>Payload for <see cref="MemoryReviewProposalType.NewSemantic"/> stored in <c>ProposedChangeJson</c>.</summary>
public sealed class NewSemanticMemoryProposalV1
{
    public string Key { get; set; } = "";
    public string Claim { get; set; } = "";
    public string? Domain { get; set; }
    public double InitialConfidence { get; set; } = 0.65d;
}

public sealed class ApproveMemoryReviewQueueItemV1Request
{
    public string? ReviewNotes { get; set; }
}

public sealed class RejectMemoryReviewQueueItemV1Request
{
    public string? Reason { get; set; }
}

public sealed class PatchMemoryReviewQueueItemV1Request
{
    public string? Title { get; set; }
    public string? Summary { get; set; }
    public string? ProposedChangeJson { get; set; }
}

public sealed class ApproveMemoryReviewQueueItemV1Response
{
    public long? SemanticMemoryId { get; set; }
    public long? ProceduralRuleId { get; set; }
}
