using FluentValidation;
using Microsoft.Extensions.Options;
using Platform.Application.Abstractions.SideLearning;
using Platform.Application.Features.SideLearning;
using Platform.Application.Configuration;
using Platform.Domain.Features.SideLearning;

namespace Platform.Application.Features.SideLearning.Sessions.Progress;

public sealed class UpdateSideLearningProgressCommandHandler(
    IValidator<UpdateSideLearningProgressCommand> validator,
    ISideLearningSessionRepository sessions,
    IOptions<PlatformWorkerOptions> workerOptions)
{
    public async Task HandleAsync(UpdateSideLearningProgressCommand command, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken).ConfigureAwait(false);

        var userId = workerOptions.Value.PrimaryUserId;
        var session = await sessions
            .GetTrackedForUserAsync(command.SessionId, userId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("Session not found.");

        if (session.Phase is not (SideLearningSessionPhase.SessionReady or SideLearningSessionPhase.InProgress))
        {
            throw new InvalidOperationException("Session is not ready for progress updates.");
        }

        var sectionIds = SideLearningSessionContentHelper.ReadSectionIds(session.SessionContentJson);
        if (sectionIds.Count == 0 || !sectionIds.Contains(command.SectionId, StringComparer.Ordinal))
        {
            throw new InvalidOperationException("Unknown section id.");
        }

        var now = DateTimeOffset.UtcNow;
        if (session.Phase == SideLearningSessionPhase.SessionReady)
        {
            session.Phase = SideLearningSessionPhase.InProgress;
        }

        session.SectionsProgressJson = SideLearningSessionContentHelper.SetSectionProgress(
            session.SectionsProgressJson,
            command.SectionId,
            command.Completed);
        session.UpdatedAt = now;

        if (SideLearningSessionContentHelper.AllSectionsComplete(session.SessionContentJson, session.SectionsProgressJson))
        {
            session.Phase = SideLearningSessionPhase.AwaitingReflection;
        }

        await sessions.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
