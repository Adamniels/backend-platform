using Platform.Application.Abstractions.Workflows;

namespace Platform.IntegrationTests.Infrastructure;

/// <summary>
/// Avoids a real Temporal connection in tests that only need a session in <c>GeneratingSession</c>.
/// </summary>
public sealed class StubTemporalWorkflowStarter : IWorkflowStarter
{
    public Task<string?> StartAsync(
        string temporalTaskQueue,
        string workflowType,
        string workflowRunId,
        object? input = null,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<string?>($"stub-workflow-{workflowRunId}");
}
