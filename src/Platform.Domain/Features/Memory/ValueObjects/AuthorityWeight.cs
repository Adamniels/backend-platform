namespace Platform.Domain.Features.Memory.ValueObjects;

/// <summary>
/// Constrained 0.0–1.0 weight aligned with the memory authority model (see docs/memory).
/// </summary>
public readonly record struct AuthorityWeight(double Value)
{
    public static AuthorityWeight ExplicitUserTruth { get; } = new(1.0);
}
