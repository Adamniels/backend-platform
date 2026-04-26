namespace Platform.Domain.Features.Memory;

/// <summary>Thrown when an operation violates memory invariants (authority, lifecycle, or append rules).</summary>
public sealed class MemoryDomainException : Exception
{
    public MemoryDomainException(string message) : base(message) { }
}
