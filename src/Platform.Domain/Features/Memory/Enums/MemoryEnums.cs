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
    NewProceduralRule = 4,
    ContradictionDetected = 5,
    ArchiveStaleSemantic = 6,
    MergeSemanticCandidates = 7,
    SupersedeSemantic = 8,
    ConflictWithExplicitProfile = 9,
    ReviseSemanticClaim = 10,
    ReviseProceduralRule = 11,
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
    Supports = 6,
    Contradicts = 7,
    Supersedes = 8,
    DerivedFrom = 9,
    ConflictsWith = 10,
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

public enum MemoryEvidencePolarity
{
    Support = 1,
    Contradict = 2,
    WeakSupport = 3,
    WeakContradict = 4,
    Supersede = 5,
}

public enum MemoryEvidenceSourceKind
{
    Unspecified = 0,
    UserAction = 1,
    Workflow = 2,
    ImportedDocument = 3,
    LlmExtraction = 4,
    SystemHeuristic = 5,
    ReviewDecision = 6,
    ExplicitProfile = 7,
}

public enum MemoryEventReliabilityClass
{
    Low = 1,
    Medium = 2,
    High = 3,
}

public enum MemoryEventPrivacyClass
{
    General = 1,
    Sensitive = 2,
}
