namespace Platform.Application.Features.Memory.Events;

/// <summary>Bounds for episodic payload storage and retrieval (trust / DB safety).</summary>
public static class MemoryEventPayloadLimits
{
    /// <summary>Maximum UTF-16 characters accepted for <c>PayloadJson</c> on ingest.</summary>
    public const int MaxPayloadJsonChars = 131_072;
}
