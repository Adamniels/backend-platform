using FluentValidation;
using Microsoft.Extensions.Options;
using Platform.Application.Abstractions.SideLearning;
using Platform.Application.Features.SideLearning;
using Platform.Application.Abstractions.WorkflowRuns;
using Platform.Application.Abstractions.Workflows;
using Platform.Application.Configuration;
using Platform.Contracts.V1.SideLearning;
using Platform.Domain.Features.SideLearning;
using Platform.Domain.Features.WorkflowRuns;

namespace Platform.Application.Features.SideLearning.Sessions.Create;

public sealed class CreateSideLearningSessionCommandHandler(
    IValidator<CreateSideLearningSessionCommand> validator,
    ISideLearningSessionRepository sessions,
    IWorkflowRunRepository runs,
    IWorkflowStarter workflowStarter,
    IWorkflowStartOptions startOptions,
    IOptions<PlatformWorkerOptions> workerOptions)
{
    public async Task<CreateSideLearningSessionV1Response> HandleAsync(
        CreateSideLearningSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken).ConfigureAwait(false);

        var userId = workerOptions.Value.PrimaryUserId;
        var sessionId = $"sl-{Guid.NewGuid():N}";
        var now = DateTimeOffset.UtcNow;
        var session = new SideLearningSession
        {
            Id = sessionId,
            UserId = userId,
            Phase = SideLearningSessionPhase.ProposingTopics,
            InitialPrompt = string.IsNullOrWhiteSpace(command.InitialPrompt) ? null : command.InitialPrompt.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
        };

        var run = await runs
            .AddPendingAsync($"Side learning: topic proposals ({sessionId})", now, cancellationToken)
            .ConfigureAwait(false);
        session.WorkflowRunId = run.Id;
        await sessions.AddAsync(session, cancellationToken).ConfigureAwait(false);

        var taskQueue = startOptions.GetDefaultTaskQueue();
        var workflowInput = new
        {
            name = $"Side learning: topic proposals ({sessionId})",
            workflowType = SideLearningWorkflowTypes.WorkflowTypeName,
            taskQueue,
            workflowRunId = run.Id,
            stage = "propose_topics",
            sessionId,
            initialPrompt = session.InitialPrompt,
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

        return new CreateSideLearningSessionV1Response(
            sessionId,
            SideLearningPhaseFormatter.ToApiString(session.Phase),
            run.Id);
    }
}
