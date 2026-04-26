namespace Platform.Application.Abstractions.Memory.Context;

/// <summary>What the caller is doing when asking for a curated memory context (v1: SQL + deterministic rank; no vector/graph).</summary>
public sealed record MemoryContextRequest(
    int UserId,
    string? TaskDescription = null,
    string? WorkflowType = null,
    string? ProjectId = null,
    string? Domain = null);
