namespace Platform.Application.Abstractions.Memory.Context;

/// <summary>What the caller is doing when asking for memory context (no ML; scoping for future retrieval rules).</summary>
public sealed record MemoryContextRequest(
    int UserId,
    string? WorkflowType = null,
    string? TaskLabel = null);
