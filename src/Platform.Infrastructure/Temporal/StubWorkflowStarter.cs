using Microsoft.Extensions.Logging;
using Platform.Application.Abstractions.Workflows;

namespace Platform.Infrastructure.Temporal;

public sealed class StubWorkflowStarter(ILogger<StubWorkflowStarter> logger) : IWorkflowStarter
{
    public Task<string?> StartAsync(
        string temporalTaskQueue,
        string workflowType,
        string workflowRunId,
        CancellationToken cancellationToken = default)
    {
        var id = $"stub-{workflowRunId}";
        logger.LogInformation(
            "Stub workflow start (Temporal not configured). Queue={Queue} Type={Type} RunId={RunId} StubId={StubId}",
            temporalTaskQueue,
            workflowType,
            workflowRunId,
            id);
        return Task.FromResult<string?>(id);
    }
}
