using Platform.Domain.Features.Memory;

namespace Platform.Domain.Features.Memory.ValueObjects;

/// <summary>0.0–1.0 per master architecture (explicit user = high authority).</summary>
public readonly record struct AuthorityWeight(double Value)
{
    public static AuthorityWeight ExplicitUserTruth { get; } = new(1.0);

    /// <summary>User approved a proposed semantic via review queue — higher than <see cref="Inferred" />, below direct explicit entry.</summary>
    public static AuthorityWeight UserApprovedSemantic { get; } = new(0.92);

    /// <summary>Review-approved procedural rule (same floor as user-approved semantics).</summary>
    public static AuthorityWeight UserApprovedProcedural { get; } = new(0.92);

    public static AuthorityWeight Inferred { get; } = new(0.55);

    /// <summary>At or above this authority, automated inferred updates must not change claim/confidence/authority (see semantic management rules).</summary>
    public static double InferredOverrideCeiling { get; } = UserApprovedSemantic.Value;

    public static bool TryCreate(double value, out AuthorityWeight weight)
    {
        if (double.IsNaN(value) || value is < MemoryValueConstraints.MinUnit or > MemoryValueConstraints.MaxUnit)
        {
            weight = default;
            return false;
        }

        weight = new AuthorityWeight(value);
        return true;
    }

    public static AuthorityWeight FromDoubleClamped(double value) =>
        newAuthority(MemoryValueConstraints.Clamp01(value));

    private static AuthorityWeight newAuthority(double v) => new(v);

    public void ThrowIfNotValid() => MemoryValueConstraints.ThrowIfOutOf01(nameof(AuthorityWeight), Value);

    public static implicit operator double(AuthorityWeight w) => w.Value;
}
