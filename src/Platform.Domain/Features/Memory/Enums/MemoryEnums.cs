namespace Platform.Domain.Features.Memory;

public enum MemoryItemType
{
    Unspecified = 0,
    ProfileFact = 1,
    Note = 2,
    Inferred = 3,
    Document = 4,
}

public enum MemoryItemStatus
{
    Draft = 0,
    Active = 1,
    Archived = 2,
    Superseded = 3,
}

public enum MemoryEventDomain
{
    Unspecified = 0,
    Learning = 1,
    Workflow = 2,
    Recommendation = 3,
    Profile = 4,
}

public enum MemoryReviewProposalType
{
    Unspecified = 0,
    NewSemantic = 1,
    AdjustConfidence = 2,
    MergeDuplicate = 3,
}

public enum MemoryReviewStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Superseded = 3,
}

public enum MemoryRelationshipType
{
    Unspecified = 0,
    InterestedIn = 1,
    WorksOn = 2,
    Uses = 3,
    Learning = 4,
    AppliedTo = 5,
}

public enum ProceduralRuleStatus
{
    Inactive = 0,
    Active = 1,
    Deprecated = 2,
}

/// <summary>Lifecycle for <c>semantic_memories</c> (distinct from <see cref="MemoryItemStatus"/> for flexibility).</summary>
public enum SemanticMemoryStatus
{
    Unknown = 0,
    Active = 1,
    Superseded = 2,
    Archived = 3,
    PendingReview = 4,
    Rejected = 5,
}

public enum MemoryConsolidationRunStatus
{
    Unknown = 0,
    Running = 1,
    Completed = 2,
    Failed = 3,
}
