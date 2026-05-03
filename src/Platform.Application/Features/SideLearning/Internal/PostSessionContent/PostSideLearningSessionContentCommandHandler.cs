using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using Platform.Application.Abstractions.SideLearning;
using Platform.Application.Configuration;
using Platform.Application.Features.Memory.ReviewQueue.CreateItem;
using Platform.Contracts.V1.SideLearning;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.SideLearning;

namespace Platform.Application.Features.SideLearning.Internal.PostSessionContent;

public sealed class PostSideLearningSessionContentCommandHandler(
    ISideLearningSessionRepository sessions,
    CreateReviewQueueItemCommandHandler createReview,
    IOptions<PlatformWorkerOptions> workerOptions)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task HandleAsync(PostSideLearningSessionContentCommand command, CancellationToken cancellationToken = default)
    {
        var session = await sessions
            .GetTrackedByIdAsync(command.SessionId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("Session not found.");

        if (session.Phase != SideLearningSessionPhase.GeneratingSession)
        {
            throw new InvalidOperationException("Session is not generating content.");
        }

        var body = command.Body;
        if (body.Sections.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("Sections must be a JSON array.");
        }

        var sectionsNode = JsonNode.Parse(body.Sections.GetRawText()) ?? new JsonArray();
        var root = new JsonObject { ["sections"] = sectionsNode };
        var now = DateTimeOffset.UtcNow;
        session.SessionContentJson = root.ToJsonString(JsonOptions);
        session.SectionsProgressJson = "{}";
        session.Phase = SideLearningSessionPhase.SessionReady;
        session.UpdatedAt = now;
        await sessions.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var userId = workerOptions.Value.PrimaryUserId;
        if (body.MemoryProposals is { ValueKind: JsonValueKind.Array } arr)
        {
            foreach (var el in arr.EnumerateArray())
            {
                var item = el.Deserialize<SideLearningMemoryProposalV1Item>(JsonOptions);
                if (item is null || string.IsNullOrWhiteSpace(item.ProposalType) || string.IsNullOrWhiteSpace(item.Title))
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
        }
    }
}
