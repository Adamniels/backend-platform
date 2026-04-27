using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Platform.Application.Abstractions.Memory.Contradictions;
using Platform.Application.Abstractions.Memory.Context;
using Platform.Application.Abstractions.Memory.Embeddings;
using Platform.Application.Features.Memory.Context;
using Platform.Application.Features.Memory.Events;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.Memory.Context;

public sealed class EfMemoryContextProvider(
    PlatformDbContext db,
    IMemoryEmbeddingGenerator embeddingGenerator,
    IMemoryVectorRecallSearch vectorRecallSearch,
    IExplicitProfileConflictDetector explicitProfileConflictDetector,
    ILogger<EfMemoryContextProvider> logger) : IMemoryContextProvider
{
    public async Task<MemoryContextV1Dto> GetContextAsync(
        MemoryContextRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var userId = request.UserId;
        var queryTokens = MemoryContextV1Scoring.Tokenize(request.TaskDescription);
        var workflow = request.WorkflowType;
        var project = request.ProjectId;
        var domain = request.Domain;

        var warnings = new List<MemoryWarningV1Dto>();
        var profile = await db.ExplicitUserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken)
            .ConfigureAwait(false);
        if (profile is null)
        {
            warnings.Add(
                new MemoryWarningV1Dto
                {
                    Code = "no_explicit_profile",
                    Message = "No explicit user profile row exists; profile slices are empty.",
                });
        }

        if (string.IsNullOrWhiteSpace(request.TaskDescription))
        {
            warnings.Add(
                new MemoryWarningV1Dto
                {
                    Code = "no_task_description",
                    Message = "Task description was empty; text relevance is neutral for ranked rows.",
                });
        }

        var profileFacts = new List<ProfileFactV1Dto>();
        var activeGoals = new List<ActiveGoalV1Dto>();
        var projects = new List<RelevantProjectV1Dto>();
        if (profile is not null)
        {
            var auth = ExplicitUserProfileContent.ExplicitUserAuthorityValue;
            foreach (var t in profile.CoreInterests ?? new List<string>())
            {
                var tm = MemoryContextV1Scoring.TextMatchRatio(queryTokens, t);
                var raw = 0.45d * auth + 0.55d * tm;
                profileFacts.Add(
                    new ProfileFactV1Dto
                    {
                        Source = "explicit.core_interest",
                        Text = t,
                        AuthorityWeight = auth,
                        RankScore = MemoryContextV1Scoring.ExplicitProfileItemRank(raw),
                    });
            }

            foreach (var t in profile.SecondaryInterests ?? new List<string>())
            {
                var tm = MemoryContextV1Scoring.TextMatchRatio(queryTokens, t);
                var raw = 0.45d * auth + 0.55d * tm;
                profileFacts.Add(
                    new ProfileFactV1Dto
                    {
                        Source = "explicit.secondary_interest",
                        Text = t,
                        AuthorityWeight = auth,
                        RankScore = MemoryContextV1Scoring.ExplicitProfileItemRank(raw),
                    });
            }

            IReadOnlyList<ProfileMemoryPreference> prefs;
            try
            {
                prefs = ExplicitUserProfileContent.ParseAndValidatePreferencesJson(
                    profile.PreferencesJson,
                    "PreferencesJson");
            }
            catch
            {
                prefs = Array.Empty<ProfileMemoryPreference>();
            }

            foreach (var p in prefs)
            {
                var line = $"{p.Key}: {p.Value}";
                var tm = MemoryContextV1Scoring.TextMatchRatio(queryTokens, p.Key, p.Value);
                var raw = 0.45d * auth + 0.55d * tm;
                profileFacts.Add(
                    new ProfileFactV1Dto
                    {
                        Source = "explicit.preference",
                        Text = line,
                        AuthorityWeight = auth,
                        RankScore = MemoryContextV1Scoring.ExplicitProfileItemRank(raw),
                    });
            }

            foreach (var g in profile.Goals ?? new List<string>())
            {
                var tm = MemoryContextV1Scoring.TextMatchRatio(queryTokens, g);
                var raw = 0.4d * auth + 0.6d * tm;
                activeGoals.Add(
                    new ActiveGoalV1Dto
                    {
                        Goal = g,
                        AuthorityWeight = auth,
                        RankScore = MemoryContextV1Scoring.ExplicitProfileItemRank(raw),
                    });
            }

            IReadOnlyList<ProfileMemoryProject> projectsParsed;
            try
            {
                projectsParsed = ExplicitUserProfileContent.ParseAndValidateActiveProjectsJson(
                    profile.ActiveProjectsJson,
                    "ActiveProjectsJson");
            }
            catch
            {
                projectsParsed = Array.Empty<ProfileMemoryProject>();
            }

            foreach (var p in projectsParsed)
            {
                var tm = MemoryContextV1Scoring.TextMatchRatio(queryTokens, p.Name, p.ExternalId);
                var pr = MemoryContextV1Scoring.ProjectRelevance(project, p.ExternalId);
                var raw = 0.4d * tm + 0.4d * pr + 0.2d * auth;
                projects.Add(
                    new RelevantProjectV1Dto
                    {
                        Name = p.Name,
                        ExternalId = p.ExternalId,
                        RankScore = MemoryContextV1Scoring.ExplicitProfileItemRank(raw),
                    });
            }
        }

        var semanticsIn = await db.SemanticMemories
            .AsNoTracking()
            .Where(
                s => s.UserId == userId &&
                    (s.Status == SemanticMemoryStatus.Active || s.Status == SemanticMemoryStatus.PendingReview))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var semanticIds = semanticsIn.Select(s => s.Id).ToList();
        Dictionary<long, (int Count, int Contradictions, IReadOnlyList<long> TopEventIds)> evidenceBySemantic = new();
        if (semanticIds.Count > 0)
        {
            var linkRows = await (
                    from ev in db.MemoryEvidences.AsNoTracking()
                    join e in db.MemoryEvents.AsNoTracking() on ev.EventId equals e.Id
                    where ev.UserId == userId && semanticIds.Contains(ev.SemanticMemoryId)
                    select new { ev.SemanticMemoryId, ev.EventId, e.OccurredAt, ev.Polarity })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            foreach (var g in linkRows.GroupBy(x => x.SemanticMemoryId))
            {
                var topIds = g.OrderByDescending(x => x.OccurredAt)
                    .Select(x => x.EventId)
                    .Distinct()
                    .Take(8)
                    .ToList();
                var contradictions = g.Count(
                    x => x.Polarity is MemoryEvidencePolarity.Contradict
                        or MemoryEvidencePolarity.WeakContradict
                        or MemoryEvidencePolarity.Supersede);
                evidenceBySemantic[g.Key] = (g.Count(), contradictions, topIds);
            }
        }

        var semanticDtos = new List<SemanticMemoryContextV1Dto>();
        foreach (var s in semanticsIn)
        {
            _ = evidenceBySemantic.TryGetValue(s.Id, out var evPack);
            var evidenceCount = evPack.Count;
            var contradictionCount = evPack.Contradictions;
            var supportingIds = evPack.TopEventIds ?? Array.Empty<long>();
            if (evidenceCount == 0)
            {
                logger.LogWarning(
                    "Semantic memory {SemanticId} (user {UserId}, key {Key}) has no linked evidence rows.",
                    s.Id,
                    userId,
                    s.Key);
            }

            var wRel = MemoryContextV1Scoring.WorkflowRelevance(workflow, null);
            var pRel = MemoryContextV1Scoring.ProjectRelevance(project, null);
            var rec = MemoryContextV1Scoring.RecencyScore(s.UpdatedAt, now, 45d);
            var tm = MemoryContextV1Scoring.TextMatchRatio(queryTokens, s.Key, s.Claim, s.Domain);
            var dm = DomainRelevance(domain, s.Domain);
            var st = MemoryContextV1Scoring.SemanticStatusFactor(s.Status);
            var rank = MemoryContextV1Scoring.CombinedLearnerRank(
                s.AuthorityWeight,
                s.Confidence,
                rec,
                wRel,
                pRel,
                tm,
                dm,
                st);
            if (contradictionCount > 0)
            {
                rank *= Math.Max(0.55d, 1d - contradictionCount * 0.08d);
            }
            semanticDtos.Add(
                new SemanticMemoryContextV1Dto
                {
                    Id = s.Id,
                    Key = s.Key,
                    Claim = s.Claim,
                    Domain = s.Domain,
                    Confidence = s.Confidence,
                    AuthorityWeight = s.AuthorityWeight,
                    Status = MemoryContextV1Scoring.SemanticStatusString(s.Status),
                    UpdatedAt = s.UpdatedAt,
                    RankScore = rank,
                    EvidenceLinkCount = evidenceCount,
                    SupportingEventIds = supportingIds,
                    LastSupportedAt = s.LastSupportedAt,
                });
        }

        semanticDtos = semanticDtos
            .OrderByDescending(s => s.RankScore * (s.AuthorityWeight < 0.8d ? 0.9d : 1d))
            .Take(32)
            .ToList();

        var events = await db.MemoryEvents
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.OccurredAt)
            .Take(400)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var eventDtos = new List<EpisodicExampleV1Dto>();
        foreach (var e in events)
        {
            var wRel = MemoryContextV1Scoring.WorkflowRelevance(workflow, e.WorkflowId);
            var pRel = MemoryContextV1Scoring.ProjectRelevance(project, e.ProjectId);
            var rec = MemoryContextV1Scoring.RecencyScore(e.OccurredAt, now, 20d);
            var payloadForRank = MemoryEventPayloadForRetrieval.TruncateForRanking(e.PayloadJson);
            var tm = MemoryContextV1Scoring.TextMatchRatio(
                queryTokens,
                e.EventType,
                payloadForRank,
                e.Domain);
            var dm = DomainRelevance(domain, e.Domain);
            const double neutralAuthority = 0.55d;
            const double eventConfidence = 0.7d;
            var rank = MemoryContextV1Scoring.CombinedLearnerRank(
                neutralAuthority,
                eventConfidence,
                rec,
                wRel,
                pRel,
                tm,
                dm,
                0.9d);
            eventDtos.Add(
                new EpisodicExampleV1Dto
                {
                    Id = e.Id,
                    EventType = e.EventType,
                    Domain = e.Domain,
                    WorkflowId = e.WorkflowId,
                    ProjectId = e.ProjectId,
                    OccurredAt = e.OccurredAt,
                    PayloadPreview = TruncatePayload(e.PayloadJson, 200),
                    RankScore = rank,
                });
        }

        eventDtos = eventDtos
            .OrderByDescending(x => x.RankScore)
            .Take(24)
            .ToList();

        var allRules = await db.ProceduralRules
            .AsNoTracking()
            .Where(
                r => r.UserId == userId && r.Status == ProceduralRuleStatus.Active)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var rulesLatest = allRules
            .GroupBy(r => (r.WorkflowType, r.RuleName))
            .Select(
                g => g.OrderByDescending(x => x.Version)
                    .First())
            .ToList();
        var ruleDtos = new List<ProceduralRuleContextV1Dto>();
        foreach (var r in rulesLatest)
        {
            var wRel = MemoryContextV1Scoring.WorkflowRelevance(workflow, r.WorkflowType);
            var rec = MemoryContextV1Scoring.RecencyScore(r.UpdatedAt, now, 120d);
            var ruleAuthority = r.AuthorityWeight;
            const double ruleConfidence = 0.85d;
            var rank = MemoryContextV1Scoring.CombinedLearnerRank(
                ruleAuthority,
                ruleConfidence,
                rec,
                wRel,
                0.45d,
                0.5d,
                0.5d,
                1d);
            rank = rank * (0.65d + 0.35d * wRel) * (1d + Math.Min(3, r.Priority) * 0.03d);
            ruleDtos.Add(
                new ProceduralRuleContextV1Dto
                {
                    Id = r.Id,
                    WorkflowType = r.WorkflowType,
                    RuleName = r.RuleName,
                    RuleContent = TruncatePayload(r.RuleContent, 2_000) ?? "",
                    Priority = r.Priority,
                    Version = r.Version,
                    Status = MemoryContextV1Scoring.ProceduralStatusString(r.Status),
                    Source = r.Source,
                    AuthorityWeight = r.AuthorityWeight,
                    RankScore = rank,
                });
        }

        ruleDtos = ruleDtos
            .OrderByDescending(r => r.RankScore)
            .Take(32)
            .ToList();

        var conflicts = new List<MemoryConflictV1Dto>();
        var explicitConflicts = explicitProfileConflictDetector.Detect(profile, semanticsIn);
        foreach (var c in explicitConflicts)
        {
            conflicts.Add(
                new MemoryConflictV1Dto
                {
                    Kind = "explicit_profile_conflict",
                    Summary = $"Semantic memory conflicts with explicit {c.Kind}: {c.ExplicitText}.",
                    RelatedEntityIds = [c.SemanticMemoryId.ToString()],
                    Severity = "Review",
                    AgainstExplicitProfile = true,
                    Confidence = c.Confidence,
                    AuthorityWeight = c.AuthorityWeight,
                });
        }

        foreach (var s in semanticDtos.Where(x => evidenceBySemantic.TryGetValue(x.Id, out var ev) && ev.Contradictions > 0))
        {
            conflicts.Add(
                new MemoryConflictV1Dto
                {
                    Kind = "contradicting_evidence",
                    Summary = $"Semantic memory «{s.Key}» has contradicting evidence.",
                    RelatedEntityIds = [s.Id.ToString()],
                    Severity = "Review",
                    Confidence = s.Confidence,
                    AuthorityWeight = s.AuthorityWeight,
                });
        }

        var semByKey = semanticsIn.GroupBy(
                s => s.Key.Trim()
                    .ToLowerInvariant())
            .Where(
                g => g.Count() > 1);
        foreach (var g in semByKey)
        {
            var claims = g.Select(
                    x => x.Claim.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToList();
            if (claims.Count <= 1)
            {
                continue;
            }

            conflicts.Add(
                new MemoryConflictV1Dto
                {
                    Kind = "duplicate_semantic_key",
                    Summary = $"Multiple distinct claims for semantic key «{g.First().Key}».",
                    RelatedEntityIds = g.Select(
                            x => x.Id.ToString())
                        .ToList(),
                    Severity = "Review",
                });
        }

        if (activeGoals.Count > 32)
        {
            activeGoals = activeGoals
                .OrderByDescending(
                    a => a.RankScore)
                .Take(32)
                .ToList();
        }

        if (profileFacts.Count > 64)
        {
            profileFacts = profileFacts
                .OrderByDescending(
                    a => a.RankScore)
                .Take(64)
                .ToList();
        }

        if (projects.Count > 32)
        {
            projects = projects
                .OrderByDescending(
                    a => a.RankScore)
                .Take(32)
                .ToList();
        }

        if (request.IncludeVectorRecall &&
            !string.IsNullOrWhiteSpace(request.TaskDescription) &&
            embeddingGenerator.Dimensions <= 0)
        {
            warnings.Add(
                new MemoryWarningV1Dto
                {
                    Code = "vector_recall_disabled",
                    Message =
                        "Vector recall is enabled but no embedding generator is configured (set MemoryVector:UseDeterministicEmbeddingGenerator or plug a real model).",
                });
        }

        var vectorRecallDtos = new List<MemoryItemVectorRecallV1Dto>();
        var vectorRecallUsed = false;
        var assemblyStage = "v1-sql";
        if (request.IncludeVectorRecall &&
            !string.IsNullOrWhiteSpace(request.TaskDescription) &&
            embeddingGenerator.Dimensions > 0)
        {
            var queryEmbedding = await embeddingGenerator
                .TryEmbedRecallQueryAsync(request.TaskDescription, cancellationToken)
                .ConfigureAwait(false);
            if (queryEmbedding is null)
            {
                warnings.Add(
                    new MemoryWarningV1Dto
                    {
                        Code = "vector_recall_unavailable",
                        Message =
                            "Vector recall was requested but no embedding could be produced (generator not configured or empty input).",
                    });
            }
            else
            {
                var hits = await vectorRecallSearch
                    .SearchMemoryItemsAsync(
                        userId,
                        queryEmbedding,
                        embeddingGenerator.ModelKey,
                        16,
                        request.ProjectId,
                        request.Domain,
                        cancellationToken)
                    .ConfigureAwait(false);
                foreach (var h in hits)
                {
                    var rank = MemoryValueConstraints.Clamp01(
                        0.55d * h.CosineSimilarity + 0.45d * h.AuthorityWeight);
                    var isDoc = string.Equals(
                        h.MemoryType,
                        nameof(MemoryItemType.Document),
                        StringComparison.OrdinalIgnoreCase);
                    vectorRecallDtos.Add(
                        new MemoryItemVectorRecallV1Dto
                        {
                            MemoryItemId = h.MemoryItemId,
                            ChunkIndex = h.ChunkIndex,
                            MemoryType = h.MemoryType,
                            Title = h.Title,
                            ContentPreview = h.ContentPreview,
                            CosineSimilarity = h.CosineSimilarity,
                            AuthorityWeight = h.AuthorityWeight,
                            RankScore = rank,
                            EmbeddingModelKey = h.EmbeddingModelKey,
                            IsDocumentEvidence = isDoc,
                            ProjectId = h.ProjectId,
                            Domain = h.Domain,
                            SourceType = h.SourceType,
                        });
                }

                vectorRecallUsed = vectorRecallDtos.Count > 0;
                assemblyStage = "v1-sql+vector";
            }
        }

        return new MemoryContextV1Dto
        {
            ProfileFacts = profileFacts.OrderByDescending(
                    a => a.RankScore)
                .ToList(),
            ActiveGoals = activeGoals,
            RelevantProjects = projects,
            SemanticMemories = semanticDtos,
            EpisodicExamples = eventDtos,
            ProceduralRules = ruleDtos,
            Conflicts = conflicts,
            Warnings = warnings,
            MemoryItemVectorRecalls = vectorRecallDtos,
            VectorRecallUsed = vectorRecallUsed,
            AssemblyStage = assemblyStage,
        };
    }

    private static double DomainRelevance(string? requestDomain, string? rowDomain)
    {
        if (string.IsNullOrWhiteSpace(requestDomain))
        {
            return 0.5d;
        }

        if (string.IsNullOrWhiteSpace(rowDomain))
        {
            return 0.4d;
        }

        return string.Equals(
            requestDomain.Trim(),
            rowDomain.Trim(),
            StringComparison.OrdinalIgnoreCase)
            ? 1d
            : 0.2d;
    }

    private static string? TruncatePayload(string? json, int max)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        if (json.Length <= max)
        {
            return json;
        }

        return json.Substring(0, max) + "…";
    }
}
