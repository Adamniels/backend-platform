namespace Platform.Domain.Features.Memory;

/// <summary>Domain-level conflict (e.g. duplicate key+domain) mapped to HTTP 409 in the API host.</summary>
public sealed class MemoryConflictException : Exception
{
    public MemoryConflictException(string message) : base(message) { }
}
