using FluentValidation;
using Platform.Application.Abstractions.WorkflowRuns;
using Platform.Application.Abstractions.Workflows;
using Platform.Application.Features.WorkflowRuns.Shared;
using Platform.Contracts.V1;
using Platform.Domain.Features.WorkflowRuns;

namespace Platform.Application.Features.WorkflowRuns.StartWorkflowRun;

public sealed class StartWorkflowRunCommandHandler(
    IValidator<StartWorkflowRunCommand> validator,
    IWorkflowRunRepository runs,
    IWorkflowStarter workflowStarter,
    IWorkflowStartOptions startOptions)
{
    public async Task<WorkflowRunSummaryDto> HandleAsync(
        StartWorkflowRunCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken).ConfigureAwait(false);

        var taskQueue = string.IsNullOrWhiteSpace(command.TaskQueue)
            ? startOptions.GetDefaultTaskQueue()
            : command.TaskQueue!;

        var now = DateTimeOffset.UtcNow;
        var run = await runs.AddPendingAsync(command.Name, now, cancellationToken).ConfigureAwait(false);

        var temporalId = await workflowStarter
            .StartAsync(taskQueue, command.WorkflowType, run.Id, cancellationToken)
            .ConfigureAwait(false);

        run.Status = temporalId is null ? WorkflowRunStatus.Failed : WorkflowRunStatus.Running;
        run.TemporalWorkflowId = temporalId;
        run.UpdatedAt = DateTimeOffset.UtcNow;
        await runs.SaveRunAfterTemporalStartAsync(run, cancellationToken).ConfigureAwait(false);

        return new WorkflowRunSummaryDto(
            run.Id,
            run.Name,
            WorkflowRunStatusFormatter.ToApiString(run.Status),
            run.UpdatedAt.ToString("O"));
    }
}
