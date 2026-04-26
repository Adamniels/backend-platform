using Platform.Domain.Features.Memory;

namespace Platform.Domain.Features.Memory.ValueObjects;

/// <summary>0.0–1.0 per master architecture (explicit user = high authority).</summary>
public readonly record struct AuthorityWeight(double Value)
{
    public static AuthorityWeight ExplicitUserTruth { get; } = new(1.0);
    public static AuthorityWeight Inferred { get; } = new(0.55);

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
