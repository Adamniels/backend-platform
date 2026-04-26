namespace Platform.Application.Abstractions.Memory.Review;

/// <summary>Outcome of approving a review item; at most one entity id is set per supported proposal type.</summary>
public sealed record MemoryReviewApprovalResult(long? SemanticMemoryId, long? ProceduralRuleId);
