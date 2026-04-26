namespace Platform.Application.Features.Memory.Context.GetMemoryContextShell;

public readonly record struct GetMemoryContextQuery(
    int UserId = 0,
    string? TaskDescription = null,
    string? WorkflowType = null,
    string? ProjectId = null,
    string? Domain = null,
    bool IncludeVectorRecall = true);
