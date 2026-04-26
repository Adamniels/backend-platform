using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.ValueObjects;

namespace Platform.Domain.Features.Memory.Entities;

public sealed class SemanticMemory
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public MemoryUser? User { get; set; }

    public string Key { get; set; } = "";
    public string Claim { get; set; } = "";
    public string? Domain { get; set; }
    public double Confidence { get; set; }
    public double AuthorityWeight { get; set; }
    public SemanticMemoryStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? LastSupportedAt { get; set; }

    public static SemanticMemory CreateInitial(
        int userId,
        string key,
        string claim,
        double confidence,
        AuthorityWeight authority,
        string? domain,
        DateTimeOffset at,
        SemanticMemoryStatus status = SemanticMemoryStatus.Active)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(claim))
        {
            throw new MemoryDomainException("Semantic memory requires non-empty key and claim.");
        }

        if (status
            is not (SemanticMemoryStatus.Active
                or SemanticMemoryStatus.PendingReview))
        {
            throw new MemoryDomainException("New semantic memory must be created in Active or PendingReview status.");
        }

        MemoryValueConstraints.ThrowIfOutOf01(nameof(confidence), confidence);
        authority.ThrowIfNotValid();

        return new SemanticMemory
        {
            UserId = userId,
            Key = key.Trim(),
            Claim = claim.Trim(),
            Domain = string.IsNullOrWhiteSpace(domain) ? null : domain.Trim(),
            Confidence = confidence,
            AuthorityWeight = authority.Value,
            Status = status,
            CreatedAt = at,
            UpdatedAt = at,
        };
    }

    /// <summary>Automated inferred updates must not mutate rows at or above the user-approval floor (see <see cref="ValueObjects.AuthorityWeight" />).</summary>
    public void ThrowIfInferredMutationBlocked()
    {
        if (this.AuthorityWeight >= global::Platform.Domain.Features.Memory.ValueObjects.AuthorityWeight.InferredOverrideCeiling)
        {
            throw new MemoryDomainException(
                "Inferred or low-trust updates cannot change this semantic: authority is at or above the user-approval floor.");
        }
    }

    public void ReinforceWithEvidence(
        double confidenceDelta,
        DateTimeOffset supportedAt,
        DateTimeOffset at,
        bool fromInferredSource = false)
    {
        if (fromInferredSource)
        {
            ThrowIfInferredMutationBlocked();
        }

        if (Status is not (SemanticMemoryStatus.Active or SemanticMemoryStatus.PendingReview))
        {
            throw new MemoryDomainException("Can only reinforce active or pending-review semantic memories.");
        }

        var next = MemoryValueConstraints.Clamp01(Confidence + confidenceDelta);
        Confidence = next;
        LastSupportedAt = supportedAt;
        UpdatedAt = at;
    }

    public void SetConfidence(double newConfidence, DateTimeOffset at)
    {
        if (Status is not (SemanticMemoryStatus.Active or SemanticMemoryStatus.PendingReview))
        {
            throw new MemoryDomainException("Can only set confidence on active or pending-review semantic memories.");
        }

        MemoryValueConstraints.ThrowIfOutOf01(nameof(newConfidence), newConfidence);
        Confidence = newConfidence;
        UpdatedAt = at;
    }

    public void SetAuthority(AuthorityWeight weight, DateTimeOffset at)
    {
        weight.ThrowIfNotValid();
        AuthorityWeight = weight.Value;
        UpdatedAt = at;
    }

    public void MarkSuperseded(DateTimeOffset at)
    {
        Status = SemanticMemoryStatus.Superseded;
        UpdatedAt = at;
    }

    public void MarkArchived(DateTimeOffset at)
    {
        if (Status is not (SemanticMemoryStatus.Active or SemanticMemoryStatus.PendingReview))
        {
            throw new MemoryDomainException("Can only archive active or pending-review semantic memories.");
        }

        Status = SemanticMemoryStatus.Archived;
        UpdatedAt = at;
    }

    public void MarkRejected(DateTimeOffset at)
    {
        if (Status is not SemanticMemoryStatus.PendingReview)
        {
            throw new MemoryDomainException("Can only reject pending-review semantic memories; use archive for active rows.");
        }

        Status = SemanticMemoryStatus.Rejected;
        UpdatedAt = at;
    }

    public void ApplyUserApprovedRevision(
        string claim,
        double confidence,
        AuthorityWeight authority,
        DateTimeOffset at)
    {
        if (Status is not (SemanticMemoryStatus.Active or SemanticMemoryStatus.PendingReview))
        {
            throw new MemoryDomainException("Can only revise active or pending-review semantic memories.");
        }

        if (string.IsNullOrWhiteSpace(claim))
        {
            throw new MemoryDomainException("Claim is required.");
        }

        MemoryValueConstraints.ThrowIfOutOf01(nameof(confidence), confidence);
        authority.ThrowIfNotValid();
        Claim = claim.Trim();
        Confidence = confidence;
        AuthorityWeight = authority.Value;
        UpdatedAt = at;
    }
}
