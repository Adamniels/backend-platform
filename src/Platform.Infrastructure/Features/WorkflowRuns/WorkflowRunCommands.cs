using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Platform.Application.Abstractions.Workflows;
using Platform.Application.Features.WorkflowRuns;
using Platform.Contracts.V1;
using Platform.Domain.Features.WorkflowRuns;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.WorkflowRuns;

public sealed class WorkflowRunCommands(
    PlatformDbContext db,
    IWorkflowStarter workflowStarter,
    IConfiguration configuration) : IWorkflowRunCommands
{
    public async Task<WorkflowRunSummaryDto> StartAsync(
        string name,
        string temporalWorkflowType,
        string temporalTaskQueue,
        CancellationToken cancellationToken = default)
    {
        var taskQueue = string.IsNullOrWhiteSpace(temporalTaskQueue)
            ? configuration["Temporal:DefaultTaskQueue"] ?? "platform"
            : temporalTaskQueue;

        var id = $"wr-{Guid.NewGuid():N}";
        var now = DateTimeOffset.UtcNow;
        var entity = new WorkflowRun
        {
            Id = id,
            Name = name,
            Status = WorkflowRunStatus.Pending,
            UpdatedAt = now,
            TemporalWorkflowId = null,
        };
        db.WorkflowRuns.Add(entity);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var temporalId = await workflowStarter
            .StartAsync(taskQueue, temporalWorkflowType, id, cancellationToken)
            .ConfigureAwait(false);

        entity.Status = temporalId is null ? WorkflowRunStatus.Failed : WorkflowRunStatus.Running;
        entity.TemporalWorkflowId = temporalId;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new WorkflowRunSummaryDto(
            entity.Id,
            entity.Name,
            WorkflowRunStatusMapper.ToApiString(entity.Status),
            entity.UpdatedAt.ToString("O"));
    }
}
