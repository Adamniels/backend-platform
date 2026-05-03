using System.Text.Json;
using Microsoft.Extensions.Options;
using Platform.Application.Abstractions.SideLearning;
using Platform.Application.Features.SideLearning;
using Platform.Application.Configuration;
using Platform.Application.Features.Memory.Documents.IngestDocumentMemory;
using Platform.Application.Features.Memory.Events.IngestEvent;
using Platform.Application.Features.Memory.ReviewQueue.CreateItem;
using Platform.Contracts.V1.Memory;
using Platform.Contracts.V1.SideLearning;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.SideLearning;

namespace Platform.Application.Features.SideLearning.Internal.PostReflectionInsights;

public sealed class PostSideLearningReflectionInsightsCommandHandler(
    ISideLearningSessionRepository sessions,
    CreateReviewQueueItemCommandHandler createReview,
    IngestMemoryEventCommandHandler ingestEvent,
    IngestDocumentMemoryCommandHandler ingestDocument,
    IOptions<PlatformWorkerOptions> workerOptions)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task HandleAsync(PostSideLearningReflectionInsightsCommand command, CancellationToken cancellationToken = default)
    {
        var session = await sessions
            .GetTrackedByIdAsync(command.SessionId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("Session not found.");

        if (session.Phase != SideLearningSessionPhase.AnalyzingReflection)
        {
            throw new InvalidOperationException("Session is not analyzing reflection.");
        }

        var userId = workerOptions.Value.PrimaryUserId;
        var proposals = command.Body.MemoryProposals ?? Array.Empty<SideLearningMemoryProposalV1Item>();
        foreach (var item in proposals)
        {
            if (string.IsNullOrWhiteSpace(item.ProposalType) || string.IsNullOrWhiteSpace(item.Title))
            {
                continue;
            }

            if (!Enum.TryParse<MemoryReviewProposalType>(item.ProposalType, ignoreCase: true, out _))
            {
                continue;
            }

            var cmd = new CreateReviewQueueItemCommand(
                userId,
                item.ProposalType,
                item.Title,
                item.Summary ?? "",
                item.ProposedChangeJson,
                item.EvidenceJson,
                item.Priority);
            await createReview.HandleAsync(cmd, cancellationToken).ConfigureAwait(false);
        }

        var sectionCount = SideLearningSessionContentHelper.ReadSectionIds(session.SessionContentJson).Count;
        var reflectionLen = session.ReflectionText?.Length ?? 0;
        var payload = JsonSerializer.Serialize(
            new
            {
                topicTitle = session.SelectedTopicTitle ?? "",
                durationMinutes = Math.Max(0, (int)Math.Round((session.UpdatedAt - session.CreatedAt).TotalMinutes)),
                sectionsCompleted = sectionCount,
                reflectionLength = reflectionLen,
            },
            JsonOptions);

        await ingestEvent
            .HandleAsync(
                new IngestMemoryEventCommand(
                    EventType: "side_learning.session_completed",
                    Domain: "learning",
                    WorkflowId: null,
                    ProjectId: null,
                    PayloadJson: payload,
                    UserId: userId,
                    OccurredAt: DateTimeOffset.UtcNow),
                cancellationToken)
            .ConfigureAwait(false);

        var docTitle =
            $"Side Learning: {session.SelectedTopicTitle ?? "session"} — {DateTime.UtcNow:yyyy-MM-dd}";
        var docContent =
            session.SessionContentJson
            + "\n\n--- Reflection ---\n\n"
            + (session.ReflectionText ?? "");

        await ingestDocument
            .HandleAsync(
                new IngestDocumentMemoryV1Request
                {
                    UserId = userId,
                    Title = docTitle,
                    Content = docContent,
                    SourceType = "side_learning",
                    ProjectId = null,
                    Domain = "learning",
                    IndexEmbeddings = true,
                },
                cancellationToken)
            .ConfigureAwait(false);

        var now = DateTimeOffset.UtcNow;
        session.Phase = SideLearningSessionPhase.Completed;
        session.UpdatedAt = now;
        await sessions.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
