namespace Platform.Domain.Features.Memory.Entities;

/// <summary>Logical subject for all memory rows in single-tenant mode (one row expected).</summary>
public sealed class MemoryPrincipal
{
    public const int SingleTenantKey = 1;

    public int Id { get; set; } = SingleTenantKey;
}
