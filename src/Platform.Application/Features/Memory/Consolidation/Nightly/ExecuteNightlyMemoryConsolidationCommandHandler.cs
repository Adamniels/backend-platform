using System.Text.Json;
using Microsoft.Extensions.Logging;
using Platform.Application.Abstractions.Memory.Consolidation;
using Platform.Application.Abstractions.Memory.Evidence;
using Platform.Application.Abstractions.Memory.Events;
using Platform.Application.Abstractions.Memory.Review;
using Platform.Application.Abstractions.Memory.Semantic;
using Platform.Application.Features.Memory.Consolidation;
using Platform.Application.Features.Memory.Review;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;
namespace Platform.Application.Features.Memory.Consolidation.Nightly;

public sealed class ExecuteNightlyMemoryConsolidationCommandHandler(
    IMemoryConsolidationRunRepository runs,
    IMemoryEventsReadRepository eventsRead,
    ISemanticMemoryService semantics,
    IMemoryReviewService reviews,
    IMemoryEvidenceReadRepository evidenceRead,
    IMemoryConsolidationPolicyProvider policy,
    ILogger<ExecuteNightlyMemoryConsolidationCommandHandler> logger)
{
    public async Task<NightlyMemoryConsolidationV1Response> HandleAsync(
        ExecuteNightlyMemoryConsolidationCommand command,
        CancellationToken cancellationToken = default)
    {
        var windowEndExclusive = command.WindowEndExclusiveUtc.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var windowEndOffset = new DateTimeOffset(windowEndExclusive);
        var windowStartOffset = windowEndOffset.AddDays(-1);
        var idempotencyKey = command.IdempotencyKey;

        var snapshot = await runs
            .GetSnapshotByIdempotencyKeyAsync(idempotencyKey, cancellationToken)
            .ConfigureAwait(false);
        if (snapshot is { Status: MemoryConsolidationRunStatus.Completed })
        {
            logger.LogInformation(
                "Consolidation idempotent hit (completed). Key={Key} RunId={RunId}",
                idempotencyKey,
                snapshot.Id);
            return Map(snapshot, fromCache: true);
        }

        if (snapshot is { Status: MemoryConsolidationRunStatus.Running })
        {
            throw new MemoryConflictException(
                "A consolidation run with this idempotency key is already in progress.");
        }

        var now = DateTimeOffset.UtcNow;
        MemoryConsolidationRun run;
        var tracked = await runs
            .GetTrackedByIdempotencyKeyAsync(idempotencyKey, cancellationToken)
            .ConfigureAwait(false);
        if (tracked is null)
        {
            run = new MemoryConsolidationRun
            {
                UserId = command.UserId,
                WindowStart = windowStartOffset,
                WindowEnd = windowEndOffset,
                IdempotencyKey = idempotencyKey,
                ProcessedEventsCount = 0,
                ProposalsCreatedCount = 0,
                AutoUpdatesCount = 0,
                Status = MemoryConsolidationRunStatus.Running,
                StartedAt = now,
            };
            await runs.AddAsync(run, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            run = tracked;
            run.WindowStart = windowStartOffset;
            run.WindowEnd = windowEndOffset;
            run.ProcessedEventsCount = 0;
            run.ProposalsCreatedCount = 0;
            run.AutoUpdatesCount = 0;
            run.Status = MemoryConsolidationRunStatus.Running;
            run.Error = null;
            run.StartedAt = now;
            run.CompletedAt = null;
            await runs.SaveTrackedAsync(run, cancellationToken).ConfigureAwait(false);
        }

        try
        {
            var events = await eventsRead
                .ListOccurredInRangeAsync(
                    command.UserId,
                    windowStartOffset,
                    windowEndOffset,
                    policy.MaxEventsPerWindow,
                    cancellationToken)
                .ConfigureAwait(false);
            run.ProcessedEventsCount = events.Count;

            var proposals = 0;
            var auto = 0;
            var groups = events
                .GroupBy(e => e.EventType.Trim(), StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() >= policy.MinOccurrencesForPattern)
                .ToList();

            foreach (var g in groups)
            {
                var eventType = g.Key;
                var list = g.OrderBy(e => e.Id).ToList();
                var semanticKey = MemoryConsolidationKeys.SemanticKeyFromEventType(eventType);
                var domain = MemoryConsolidationKeys.PickDomainMode(list);
                var fingerprint = MemoryConsolidationKeys.ProposalFingerprint(
                    command.UserId,
                    command.WindowEndExclusiveUtc,
                    eventType);

                var semantic = await semantics
                    .FindActiveOrPendingByKeyDomainAsync(command.UserId, semanticKey, domain, cancellationToken)
                    .ConfigureAwait(false);
                if (semantic is not null)
                {
                    if (semantic.AuthorityWeight >= global::Platform.Domain.Features.Memory.ValueObjects.AuthorityWeight.InferredOverrideCeiling)
                    {
                        logger.LogDebug(
                            "Skip auto-reinforce for semantic {SemanticId}: authority {Authority}",
                            semantic.Id,
                            semantic.AuthorityWeight);
                        continue;
                    }

                    var evidenceEvent = list[0];
                    var exists = await evidenceRead
                        .ExistsForSemanticAndEventAsync(
                            command.UserId,
                            semantic.Id,
                            evidenceEvent.Id,
                            cancellationToken)
                        .ConfigureAwait(false);
                    if (exists)
                    {
                        logger.LogDebug(
                            "Skip reinforce; evidence already linked. Semantic={SemanticId} Event={EventId}",
                            semantic.Id,
                            evidenceEvent.Id);
                        continue;
                    }

                    await semantics
                        .AttachEvidenceAsync(
                            semantic.Id,
                            command.UserId,
                            evidenceEvent.Id,
                            strength: 0.55d,
                            reason: "nightly_consolidation_v1",
                            fromInferredSource: true,
                            reinforce: true,
                            reinforceConfidenceDelta: policy.ReinforceConfidenceDelta,
                            eventOccurredAtForReinforce: evidenceEvent.OccurredAt,
                            cancellationToken)
                        .ConfigureAwait(false);
                    auto++;
                    continue;
                }

                if (await reviews
                    .HasPendingWithEvidenceSubstringAsync(command.UserId, fingerprint, cancellationToken)
                    .ConfigureAwait(false))
                {
                    logger.LogInformation(
                        "Skip duplicate proposal fingerprint {Fingerprint} for user {UserId}",
                        fingerprint,
                        command.UserId);
                    continue;
                }

                var claim =
                    $"Observed {list.Count} similar events (`{eventType}`) in the consolidation window. " +
                    "If this reflects a stable preference, approve to store it as semantic memory.";
                var proposal = new NewSemanticMemoryProposalV1
                {
                    Key = semanticKey,
                    Claim = claim,
                    Domain = domain,
                    InitialConfidence = policy.ProposalInitialConfidence,
                };
                var evidenceJson = JsonSerializer.Serialize(
                    new
                    {
                        consolidationFingerprint = fingerprint,
                        source = "nightly_v1",
                        eventType,
                        windowEndExclusive = command.WindowEndExclusiveUtc.ToString("yyyy-MM-dd", null),
                        eventCount = list.Count,
                    });

                var item = MemoryReviewQueueItem.Propose(
                    command.UserId,
                    MemoryReviewProposalType.NewSemantic,
                    title: $"Consolidation: repeated {eventType}",
                    summary: "Automated nightly consolidation proposes a new semantic from episodic repetition.",
                    proposedChangeJson: MemoryReviewProposalJson.SerializeNewSemantic(proposal),
                    evidenceJson,
                    policy.ReviewQueuePriority,
                    now);
                _ = await reviews.CreatePendingAsync(item, cancellationToken).ConfigureAwait(false);
                proposals++;
            }

            run.ProposalsCreatedCount = proposals;
            run.AutoUpdatesCount = auto;
            run.Status = MemoryConsolidationRunStatus.Completed;
            run.CompletedAt = DateTimeOffset.UtcNow;
            run.Error = null;
            await runs.SaveTrackedAsync(run, cancellationToken).ConfigureAwait(false);
            logger.LogInformation(
                "Consolidation completed RunId={RunId} Events={Events} Auto={Auto} Proposals={Proposals}",
                run.Id,
                run.ProcessedEventsCount,
                auto,
                proposals);
            return Map(run, fromCache: false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Consolidation failed for idempotency key {Key}", idempotencyKey);
            run.Status = MemoryConsolidationRunStatus.Failed;
            run.Error = ex.Message;
            run.CompletedAt = DateTimeOffset.UtcNow;
            await runs.SaveTrackedAsync(run, cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    private static NightlyMemoryConsolidationV1Response Map(MemoryConsolidationRun run, bool fromCache) =>
        new()
        {
            RunId = run.Id,
            IdempotencyKey = run.IdempotencyKey,
            Status = run.Status.ToString(),
            WindowStart = run.WindowStart,
            WindowEnd = run.WindowEnd,
            ProcessedEventsCount = run.ProcessedEventsCount,
            ProposalsCreatedCount = run.ProposalsCreatedCount,
            AutoUpdatesCount = run.AutoUpdatesCount,
            Error = run.Error,
            FromCache = fromCache,
        };
}
