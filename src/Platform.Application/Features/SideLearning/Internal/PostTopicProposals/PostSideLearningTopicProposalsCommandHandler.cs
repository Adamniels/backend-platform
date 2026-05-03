using System.Text.Json;
using Platform.Application.Abstractions.SideLearning;
using Platform.Contracts.V1.SideLearning;
using Platform.Domain.Features.SideLearning;

namespace Platform.Application.Features.SideLearning.Internal.PostTopicProposals;

public sealed class PostSideLearningTopicProposalsCommandHandler(ISideLearningSessionRepository sessions)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task HandleAsync(PostSideLearningTopicProposalsCommand command, CancellationToken cancellationToken = default)
    {
        var session = await sessions
            .GetTrackedByIdAsync(command.SessionId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("Session not found.");

        if (session.Phase != SideLearningSessionPhase.ProposingTopics)
        {
            throw new InvalidOperationException("Session is not proposing topics.");
        }

        var topics = command.Body.Topics ?? Array.Empty<SideLearningTopicProposalV1Item>();
        var normalized = topics
            .Where(t => !string.IsNullOrWhiteSpace(t.Title))
            .Select(t => new
            {
                title = t.Title!.Trim(),
                rationale = t.Rationale?.Trim() ?? "",
                estimatedMinutes = t.EstimatedMinutes ?? 0,
                difficulty = t.Difficulty?.Trim() ?? "",
                targetSkillGap = t.TargetSkillGap?.Trim() ?? "",
            })
            .ToList();

        if (normalized.Count == 0)
        {
            throw new InvalidOperationException("At least one topic with a title is required.");
        }

        var now = DateTimeOffset.UtcNow;
        session.TopicProposalsJson = JsonSerializer.Serialize(normalized, JsonOptions);
        session.Phase = SideLearningSessionPhase.AwaitingTopicSelection;
        session.UpdatedAt = now;
        await sessions.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
