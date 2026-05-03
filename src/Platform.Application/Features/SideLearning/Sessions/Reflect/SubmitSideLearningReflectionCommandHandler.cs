using FluentValidation;
using Microsoft.Extensions.Options;
using Platform.Application.Abstractions.SideLearning;
using Platform.Application.Features.SideLearning;
using Platform.Application.Abstractions.WorkflowRuns;
using Platform.Application.Abstractions.Workflows;
using Platform.Application.Configuration;
using Platform.Domain.Features.SideLearning;
using Platform.Domain.Features.WorkflowRuns;

namespace Platform.Application.Features.SideLearning.Sessions.Reflect;

public sealed class SubmitSideLearningReflectionCommandHandler(
    IValidator<SubmitSideLearningReflectionCommand> validator,
    ISideLearningSessionRepository sessions,
    IWorkflowRunRepository runs,
    IWorkflowStarter workflowStarter,
    IWorkflowStartOptions startOptions,
    IOptions<PlatformWorkerOptions> workerOptions)
{
    public async Task HandleAsync(SubmitSideLearningReflectionCommand command, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken).ConfigureAwait(false);

        var userId = workerOptions.Value.PrimaryUserId;
        var session = await sessions
            .GetTrackedForUserAsync(command.SessionId, userId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("Session not found.");

        if (session.Phase != SideLearningSessionPhase.AwaitingReflection)
        {
            throw new InvalidOperationException("Session is not awaiting reflection.");
        }

        var now = DateTimeOffset.UtcNow;
        session.ReflectionText = command.Reflection.Trim();
        session.Phase = SideLearningSessionPhase.AnalyzingReflection;
        session.UpdatedAt = now;

        var run = await runs
            .AddPendingAsync($"Side learning: analyze reflection ({session.Id})", now, cancellationToken)
            .ConfigureAwait(false);
        session.WorkflowRunId = run.Id;
        await sessions.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var taskQueue = startOptions.GetDefaultTaskQueue();
        var workflowInput = new
        {
            name = $"Side learning: analyze reflection ({session.Id})",
            workflowType = SideLearningWorkflowTypes.WorkflowTypeName,
            taskQueue,
            workflowRunId = run.Id,
            stage = "analyze_reflection",
            sessionId = session.Id,
            topicTitle = session.SelectedTopicTitle,
            reflectionText = session.ReflectionText,
            sessionContentJson = session.SessionContentJson,
        };

        var temporalId = await workflowStarter
            .StartAsync(taskQueue, SideLearningWorkflowTypes.WorkflowTypeName, run.Id, workflowInput, cancellationToken)
            .ConfigureAwait(false);

        run.Status = temporalId is null ? WorkflowRunStatus.Failed : WorkflowRunStatus.Running;
        run.TemporalWorkflowId = temporalId;
        run.UpdatedAt = DateTimeOffset.UtcNow;
        await runs.SaveRunAfterTemporalStartAsync(run, cancellationToken).ConfigureAwait(false);

        if (temporalId is null)
        {
            session.Phase = SideLearningSessionPhase.Failed;
            session.UpdatedAt = DateTimeOffset.UtcNow;
            await sessions.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
