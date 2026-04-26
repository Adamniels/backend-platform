using Platform.Domain.Features.Memory;

namespace Platform.Application.Features.Memory.Semantic.CreateSemanticMemory;

public sealed record CreateSemanticMemoryCommand(
    int UserId,
    string Key,
    string Claim,
    double Confidence,
    double? AuthorityWeight,
    string? Domain,
    string? Status,
    long EventId,
    double EvidenceStrength,
    string? EvidenceReason);

public static class SemanticMemoryInitialStatus
{
    public static SemanticMemoryStatus Parse(string? status) =>
        status?.Trim()
            .ToLowerInvariant() switch
        {
            null or "" or "active" => SemanticMemoryStatus.Active,
            "pending" or "pendingreview" => SemanticMemoryStatus.PendingReview,
            _ => throw new MemoryDomainException("Status must be Active or Pending (pending review)."),
        };
}
