namespace Platform.Application.Features.Memory.Exceptions;

/// <summary>
/// Raised by Application-layer memory handlers when a domain conflict occurs.
/// Maps to HTTP 409 in the global exception handler without exposing Domain types to the Api host.
/// </summary>
public sealed class MemoryApplicationException : Exception
{
    public MemoryApplicationException(string message) : base(message) { }
}
