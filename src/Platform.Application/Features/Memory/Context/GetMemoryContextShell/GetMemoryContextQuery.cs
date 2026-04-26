namespace Platform.Application.Features.Memory.Context.GetMemoryContextShell;

public readonly record struct GetMemoryContextQuery(
    int UserId = 1,
    string? WorkflowType = null,
    string? TaskLabel = null);
