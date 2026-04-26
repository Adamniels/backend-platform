namespace Platform.Domain.Features.Memory.Entities;

/// <summary>One row per logical end-user. Single-tenant: seed <see cref="DefaultId"/>.</summary>
public sealed class MemoryUser
{
    public const int DefaultId = 1;

    public int Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
