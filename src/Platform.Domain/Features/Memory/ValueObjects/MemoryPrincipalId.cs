namespace Platform.Domain.Features.Memory.ValueObjects;

/// <summary>Single-tenant subject key; use <see cref="SingleTenant"/> in production until multi-user is introduced.</summary>
public readonly record struct MemoryPrincipalId(int Value)
{
    public static MemoryPrincipalId SingleTenant { get; } = new(1);
}
