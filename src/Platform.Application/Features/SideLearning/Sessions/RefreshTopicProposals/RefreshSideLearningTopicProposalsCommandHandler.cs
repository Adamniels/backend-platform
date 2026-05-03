using FluentValidation;
using Microsoft.Extensions.Options;
using Platform.Application.Abstractions.SideLearning;
using Platform.Application.Features.SideLearning;
using Platform.Application.Abstractions.WorkflowRuns;
using Platform.Application.Abstractions.Workflows;
using Platform.Application.Configuration;
using Platform.Domain.Features.SideLearning;
using Platform.Domain.Features.WorkflowRuns;

namespace Platform.Application.Features.SideLearning.Sessions.RefreshTopicProposals;

public sealed class RefreshSideLearningTopicProposalsCommandHandler(
    IValidator<RefreshSideLearningTopicProposalsCommand> validator,
    ISideLearningSessionRepository sessions,
    IWorkflowRunRepository runs,
    IWorkflowStarter workflowStarter,
    IWorkflowStartOptions startOptions,
    IOptions<PlatformWorkerOptions> workerOptions)
{
    private const int MaxInitialPromptLength = 4096;

    public async Task HandleAsync(RefreshSideLearningTopicProposalsCommand command, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken).ConfigureAwait(false);

        var userId = workerOptions.Value.PrimaryUserId;
        var session = await sessions
            .GetTrackedForUserAsync(command.SessionId, userId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("Session not found.");

        if (session.Phase != SideLearningSessionPhase.AwaitingTopicSelection)
        {
            throw new InvalidOperationException("Session is not awaiting topic selection.");
        }

        var now = DateTimeOffset.UtcNow;
        session.InitialPrompt = MergeInitialPrompt(session.InitialPrompt, command.Feedback);
        session.TopicProposalsJson = "[]";
        session.SelectedTopicTitle = null;
        session.SelectedTopicReason = null;
        session.Phase = SideLearningSessionPhase.ProposingTopics;
        session.UpdatedAt = now;

        var run = await runs
            .AddPendingAsync($"Side learning: topic proposals ({session.Id})", now, cancellationToken)
            .ConfigureAwait(false);
        session.WorkflowRunId = run.Id;
        await sessions.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var taskQueue = startOptions.GetDefaultTaskQueue();
        var workflowInput = new
        {
            name = $"Side learning: topic proposals ({session.Id})",
            workflowType = SideLearningWorkflowTypes.WorkflowTypeName,
            taskQueue,
            workflowRunId = run.Id,
            stage = "propose_topics",
            sessionId = session.Id,
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
    }

    private static string? MergeInitialPrompt(string? existing, string? feedback)
    {
        if (string.IsNullOrWhiteSpace(feedback))
        {
            return existing;
        }

        var fb = feedback.Trim();
        var basePrompt = string.IsNullOrWhiteSpace(existing) ? "" : existing.Trim();
        var merged = string.IsNullOrEmpty(basePrompt)
            ? fb
            : $"{basePrompt}\n\n[Topic proposal feedback]\n{fb}";
        return merged.Length <= MaxInitialPromptLength ? merged : merged[..MaxInitialPromptLength];
    }
}
