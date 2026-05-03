using Microsoft.Extensions.Options;
using Platform.Application.Abstractions.SideLearning;
using Platform.Application.Features.SideLearning;
using Platform.Application.Configuration;
using Platform.Contracts.V1.SideLearning;
using Platform.Domain.Features.SideLearning;

namespace Platform.Application.Features.SideLearning.Sessions.Get;

public sealed class GetSideLearningSessionQueryHandler(
    ISideLearningSessionRepository sessions,
    IOptions<PlatformWorkerOptions> workerOptions)
{
    public async Task<SideLearningSessionV1Dto?> HandleAsync(
        GetSideLearningSessionQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = workerOptions.Value.PrimaryUserId;
        var session = await sessions
            .GetTrackedForUserAsync(query.SessionId, userId, cancellationToken)
            .ConfigureAwait(false);
        if (session is null)
        {
            return null;
        }

        return ToDto(session);
    }

    private static SideLearningSessionV1Dto ToDto(SideLearningSession s) =>
        new(
            s.Id,
            SideLearningPhaseFormatter.ToApiString(s.Phase),
            s.InitialPrompt,
            s.SelectedTopicTitle,
            s.SelectedTopicReason,
            s.TopicProposalsJson,
            s.SessionContentJson,
            s.SectionsProgressJson,
            s.ReflectionText,
            s.WorkflowRunId,
            s.CreatedAt.ToString("O"),
            s.UpdatedAt.ToString("O"));
}
