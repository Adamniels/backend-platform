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

public sealed class ContradictionDetectedProposalV1
{
    public long SemanticMemoryId { get; set; }
    public string Key { get; set; } = "";
    public string Claim { get; set; } = "";
    public double Confidence { get; set; }
    public double SupportScore { get; set; }
    public double ContradictionScore { get; set; }
}

public sealed class ArchiveStaleSemanticProposalV1
{
    public long SemanticMemoryId { get; set; }
    public string Key { get; set; } = "";
    public string Claim { get; set; } = "";
    public double CurrentConfidence { get; set; }
    public DateTimeOffset? LastSupportedAt { get; set; }
}

public sealed class MergeSemanticCandidatesProposalV1
{
    public IReadOnlyList<long> SourceSemanticIds { get; set; } = Array.Empty<long>();
    public long CanonicalSemanticId { get; set; }
    public string ResultingClaim { get; set; } = "";
    public string? Domain { get; set; }
}

public sealed class ConflictWithExplicitProfileProposalV1
{
    public long SemanticMemoryId { get; set; }
    public string Key { get; set; } = "";
    public string Claim { get; set; } = "";
    public string ExplicitKind { get; set; } = "";
    public string ExplicitText { get; set; } = "";
}

public sealed class SupersedeSemanticProposalV1
{
    public long SupersededSemanticId { get; set; }
    public long CanonicalSemanticId { get; set; }
    public string? Reason { get; set; }
}

public sealed class ReviseSemanticClaimProposalV1
{
    public long SemanticMemoryId { get; set; }
    public string NewClaim { get; set; } = "";
    public string? NewDomain { get; set; }
    public double? NewConfidence { get; set; }
}

public sealed class ReviseProceduralRuleProposalV1
{
    public long BasisRuleId { get; set; }
    public string RuleContent { get; set; } = "";
    public string Source { get; set; } = "";
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
