using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Platform.Application.Abstractions.Workflows;
using Temporalio.Client;

namespace Platform.Infrastructure.Temporal;

public sealed class TemporalWorkflowStarter(IConfiguration configuration, ILogger<TemporalWorkflowStarter> logger)
    : IWorkflowStarter
{
    private TemporalClient? _client;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public async Task<string?> StartAsync(
        string temporalTaskQueue,
        string workflowType,
        string workflowRunId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await GetOrCreateClientAsync(cancellationToken).ConfigureAwait(false);
            var workflowId = $"platform-{workflowRunId}";
            await client.StartWorkflowAsync(
                    workflowType,
                    [workflowRunId],
                    new WorkflowOptions(id: workflowId, taskQueue: temporalTaskQueue))
                .ConfigureAwait(false);
            logger.LogInformation(
                "Temporal workflow started. WorkflowId={WorkflowId} Type={Type} Queue={Queue}",
                workflowId,
                workflowType,
                temporalTaskQueue);
            return workflowId;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Temporal workflow start failed for {WorkflowType}", workflowType);
            return null;
        }
    }

    private async Task<TemporalClient> GetOrCreateClientAsync(CancellationToken cancellationToken)
    {
        if (_client is not null)
        {
            return _client;
        }

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_client is not null)
            {
                return _client;
            }

            var address = configuration["Temporal:Address"] ?? "localhost:7233";
            var ns = configuration["Temporal:Namespace"] ?? "default";
            _client = await TemporalClient.ConnectAsync(new(address) { Namespace = ns }).ConfigureAwait(false);
            return _client;
        }
        finally
        {
            _gate.Release();
        }
    }

}
