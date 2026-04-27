using System.Text.Json;
using System.Text.Json.Serialization;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory;

namespace Platform.Application.Features.Memory.Review;

public static class MemoryReviewProposalJson
{
    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public const string NewSemanticKind = "NewSemantic";
    public const string NewProceduralRuleKind = "NewProceduralRule";
    public const string ContradictionDetectedKind = "ContradictionDetected";
    public const string ArchiveStaleSemanticKind = "ArchiveStaleSemantic";
    public const string MergeSemanticCandidatesKind = "MergeSemanticCandidates";
    public const string ConflictWithExplicitProfileKind = "ConflictWithExplicitProfile";
    public const string SupersedeSemanticKind = "SupersedeSemantic";
    public const string ReviseSemanticClaimKind = "ReviseSemanticClaim";
    public const string ReviseProceduralRuleKind = "ReviseProceduralRule";

    public static void ValidateForProposalType(MemoryReviewProposalType proposalType, string? proposedChangeJson)
    {
        switch (proposalType)
        {
            case MemoryReviewProposalType.NewSemantic:
                _ = ParseNewSemantic(proposedChangeJson);
                break;
            case MemoryReviewProposalType.NewProceduralRule:
                _ = ParseNewProceduralRule(proposedChangeJson);
                break;
            case MemoryReviewProposalType.ContradictionDetected:
                _ = ParseContradictionDetected(proposedChangeJson);
                break;
            case MemoryReviewProposalType.ArchiveStaleSemantic:
                _ = ParseArchiveStaleSemantic(proposedChangeJson);
                break;
            case MemoryReviewProposalType.MergeDuplicate:
            case MemoryReviewProposalType.MergeSemanticCandidates:
                _ = ParseMergeSemanticCandidates(proposedChangeJson);
                break;
            case MemoryReviewProposalType.ConflictWithExplicitProfile:
                _ = ParseConflictWithExplicitProfile(proposedChangeJson);
                break;
            case MemoryReviewProposalType.SupersedeSemantic:
                _ = ParseSupersedeSemantic(proposedChangeJson);
                break;
            case MemoryReviewProposalType.ReviseSemanticClaim:
                _ = ParseReviseSemanticClaim(proposedChangeJson);
                break;
            case MemoryReviewProposalType.ReviseProceduralRule:
                _ = ParseReviseProceduralRule(proposedChangeJson);
                break;
            default:
                throw new MemoryDomainException($"Unsupported proposal type {proposalType}.");
        }
    }

    public static string SerializeNewSemantic(NewSemanticMemoryProposalV1 p) =>
        JsonSerializer.Serialize(
            new NewSemanticEnvelope(NewSemanticKind, p.Key, p.Claim, p.Domain, p.InitialConfidence),
            WriteOptions);

    public static NewSemanticMemoryProposalV1 ParseNewSemantic(string? proposedChangeJson)
    {
        if (string.IsNullOrWhiteSpace(proposedChangeJson))
        {
            throw new MemoryDomainException("ProposedChangeJson is required for a semantic proposal.");
        }

        try
        {
            using var doc = JsonDocument.Parse(proposedChangeJson);
            var root = doc.RootElement;
            var kind = root.TryGetProperty("kind", out var k) ? k.GetString() : null;
            if (!string.Equals(kind, NewSemanticKind, StringComparison.OrdinalIgnoreCase))
            {
                throw new MemoryDomainException("Unsupported proposal kind in ProposedChangeJson.");
            }

            var key = root.TryGetProperty("key", out var ke) ? ke.GetString() : null;
            var claim = root.TryGetProperty("claim", out var cl) ? cl.GetString() : null;
            var domain = root.TryGetProperty("domain", out var d) && d.ValueKind != JsonValueKind.Null
                ? d.GetString()
                : null;
            var conf = root.TryGetProperty("initialConfidence", out var c) && c.TryGetDouble(out var cd)
                ? cd
                : 0.65d;
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(claim))
            {
                throw new MemoryDomainException("Semantic proposal requires key and claim.");
            }

            return new NewSemanticMemoryProposalV1
            {
                Key = key!,
                Claim = claim!,
                Domain = domain,
                InitialConfidence = conf,
            };
        }
        catch (JsonException ex)
        {
            throw new MemoryDomainException($"Invalid proposal JSON: {ex.Message}");
        }
    }

    public static string SerializeNewProceduralRule(NewProceduralRuleMemoryProposalV1 p) =>
        JsonSerializer.Serialize(
            new NewProceduralRuleEnvelope(
                NewProceduralRuleKind,
                p.WorkflowType,
                p.RuleName,
                p.RuleContent,
                p.Priority,
                p.Source,
                p.AuthorityWeight,
                p.BasisRuleId),
            WriteOptions);

    public static string SerializeContradictionDetected(ContradictionDetectedProposalV1 p) =>
        JsonSerializer.Serialize(
            new ContradictionDetectedEnvelope(
                ContradictionDetectedKind,
                p.SemanticMemoryId,
                p.Key,
                p.Claim,
                p.Confidence,
                p.SupportScore,
                p.ContradictionScore),
            WriteOptions);

    public static ContradictionDetectedProposalV1 ParseContradictionDetected(string? proposedChangeJson)
    {
        using var root = ParseKind(proposedChangeJson, ContradictionDetectedKind);
        return new ContradictionDetectedProposalV1
        {
            SemanticMemoryId = root.RootElement.GetProperty("semanticMemoryId").GetInt64(),
            Key = root.RootElement.TryGetProperty("key", out var k) ? k.GetString() ?? "" : "",
            Claim = root.RootElement.TryGetProperty("claim", out var c) ? c.GetString() ?? "" : "",
            Confidence = root.RootElement.TryGetProperty("confidence", out var conf) && conf.TryGetDouble(out var cd) ? cd : 0d,
            SupportScore = root.RootElement.TryGetProperty("supportScore", out var ss) && ss.TryGetDouble(out var sd) ? sd : 0d,
            ContradictionScore = root.RootElement.TryGetProperty("contradictionScore", out var cs) && cs.TryGetDouble(out var ccd) ? ccd : 0d,
        };
    }

    public static string SerializeArchiveStaleSemantic(ArchiveStaleSemanticProposalV1 p) =>
        JsonSerializer.Serialize(
            new ArchiveStaleSemanticEnvelope(
                ArchiveStaleSemanticKind,
                p.SemanticMemoryId,
                p.Key,
                p.Claim,
                p.CurrentConfidence,
                p.LastSupportedAt),
            WriteOptions);

    public static ArchiveStaleSemanticProposalV1 ParseArchiveStaleSemantic(string? proposedChangeJson)
    {
        using var root = ParseKind(proposedChangeJson, ArchiveStaleSemanticKind);
        DateTimeOffset? lastSupportedAt = null;
        if (root.RootElement.TryGetProperty("lastSupportedAt", out var ls) &&
            ls.ValueKind != JsonValueKind.Null &&
            ls.TryGetDateTimeOffset(out var dto))
        {
            lastSupportedAt = dto;
        }

        return new ArchiveStaleSemanticProposalV1
        {
            SemanticMemoryId = root.RootElement.GetProperty("semanticMemoryId").GetInt64(),
            Key = root.RootElement.TryGetProperty("key", out var k) ? k.GetString() ?? "" : "",
            Claim = root.RootElement.TryGetProperty("claim", out var c) ? c.GetString() ?? "" : "",
            CurrentConfidence = root.RootElement.TryGetProperty("currentConfidence", out var conf) && conf.TryGetDouble(out var cd) ? cd : 0d,
            LastSupportedAt = lastSupportedAt,
        };
    }

    public static string SerializeMergeSemanticCandidates(MergeSemanticCandidatesProposalV1 p) =>
        JsonSerializer.Serialize(
            new MergeSemanticCandidatesEnvelope(
                MergeSemanticCandidatesKind,
                p.SourceSemanticIds,
                p.CanonicalSemanticId,
                p.ResultingClaim,
                p.Domain),
            WriteOptions);

    public static MergeSemanticCandidatesProposalV1 ParseMergeSemanticCandidates(string? proposedChangeJson)
    {
        using var root = ParseKind(proposedChangeJson, MergeSemanticCandidatesKind);
        var ids = root.RootElement.TryGetProperty("sourceSemanticIds", out var idsProp) &&
            idsProp.ValueKind == JsonValueKind.Array
            ? idsProp.EnumerateArray().Select(x => x.GetInt64()).ToList()
            : new List<long>();
        return new MergeSemanticCandidatesProposalV1
        {
            SourceSemanticIds = ids,
            CanonicalSemanticId = root.RootElement.GetProperty("canonicalSemanticId").GetInt64(),
            ResultingClaim = root.RootElement.TryGetProperty("resultingClaim", out var rc) ? rc.GetString() ?? "" : "",
            Domain = root.RootElement.TryGetProperty("domain", out var d) && d.ValueKind != JsonValueKind.Null ? d.GetString() : null,
        };
    }

    public static string SerializeConflictWithExplicitProfile(ConflictWithExplicitProfileProposalV1 p) =>
        JsonSerializer.Serialize(
            new ConflictWithExplicitProfileEnvelope(
                ConflictWithExplicitProfileKind,
                p.SemanticMemoryId,
                p.Key,
                p.Claim,
                p.ExplicitKind,
                p.ExplicitText),
            WriteOptions);

    public static ConflictWithExplicitProfileProposalV1 ParseConflictWithExplicitProfile(string? proposedChangeJson)
    {
        using var root = ParseKind(proposedChangeJson, ConflictWithExplicitProfileKind);
        var parsed = new ConflictWithExplicitProfileProposalV1
        {
            SemanticMemoryId = root.RootElement.GetProperty("semanticMemoryId").GetInt64(),
            Key = root.RootElement.TryGetProperty("key", out var k) ? k.GetString() ?? "" : "",
            Claim = root.RootElement.TryGetProperty("claim", out var c) ? c.GetString() ?? "" : "",
            ExplicitKind = root.RootElement.TryGetProperty("explicitKind", out var ek) ? ek.GetString() ?? "" : "",
            ExplicitText = root.RootElement.TryGetProperty("explicitText", out var et) ? et.GetString() ?? "" : "",
        };
        if (parsed.SemanticMemoryId <= 0 || string.IsNullOrWhiteSpace(parsed.ExplicitText))
        {
            throw new MemoryDomainException("ConflictWithExplicitProfile proposal requires semanticMemoryId and explicitText.");
        }

        return parsed;
    }

    public static string SerializeSupersedeSemantic(SupersedeSemanticProposalV1 p) =>
        JsonSerializer.Serialize(
            new SupersedeSemanticEnvelope(
                SupersedeSemanticKind,
                p.SupersededSemanticId,
                p.CanonicalSemanticId,
                p.Reason),
            WriteOptions);

    public static SupersedeSemanticProposalV1 ParseSupersedeSemantic(string? proposedChangeJson)
    {
        using var root = ParseKind(proposedChangeJson, SupersedeSemanticKind);
        var parsed = new SupersedeSemanticProposalV1
        {
            SupersededSemanticId = root.RootElement.GetProperty("supersededSemanticId").GetInt64(),
            CanonicalSemanticId = root.RootElement.GetProperty("canonicalSemanticId").GetInt64(),
            Reason = root.RootElement.TryGetProperty("reason", out var r) && r.ValueKind != JsonValueKind.Null
                ? r.GetString()
                : null,
        };
        if (parsed.SupersededSemanticId <= 0 || parsed.CanonicalSemanticId <= 0)
        {
            throw new MemoryDomainException("SupersedeSemantic proposal requires valid semantic ids.");
        }

        return parsed;
    }

    public static string SerializeReviseSemanticClaim(ReviseSemanticClaimProposalV1 p) =>
        JsonSerializer.Serialize(
            new ReviseSemanticClaimEnvelope(
                ReviseSemanticClaimKind,
                p.SemanticMemoryId,
                p.NewClaim,
                p.NewDomain,
                p.NewConfidence),
            WriteOptions);

    public static ReviseSemanticClaimProposalV1 ParseReviseSemanticClaim(string? proposedChangeJson)
    {
        using var root = ParseKind(proposedChangeJson, ReviseSemanticClaimKind);
        double? conf = null;
        if (root.RootElement.TryGetProperty("newConfidence", out var nc) &&
            nc.ValueKind != JsonValueKind.Null &&
            nc.TryGetDouble(out var parsed))
        {
            conf = parsed;
        }

        var parsedProposal = new ReviseSemanticClaimProposalV1
        {
            SemanticMemoryId = root.RootElement.GetProperty("semanticMemoryId").GetInt64(),
            NewClaim = root.RootElement.TryGetProperty("newClaim", out var claim) ? claim.GetString() ?? "" : "",
            NewDomain = root.RootElement.TryGetProperty("newDomain", out var d) && d.ValueKind != JsonValueKind.Null
                ? d.GetString()
                : null,
            NewConfidence = conf,
        };
        if (parsedProposal.SemanticMemoryId <= 0 || string.IsNullOrWhiteSpace(parsedProposal.NewClaim))
        {
            throw new MemoryDomainException("ReviseSemanticClaim proposal requires semanticMemoryId and newClaim.");
        }

        return parsedProposal;
    }

    public static string SerializeReviseProceduralRule(ReviseProceduralRuleProposalV1 p) =>
        JsonSerializer.Serialize(
            new ReviseProceduralRuleEnvelope(
                ReviseProceduralRuleKind,
                p.BasisRuleId,
                p.RuleContent,
                p.Source),
            WriteOptions);

    public static ReviseProceduralRuleProposalV1 ParseReviseProceduralRule(string? proposedChangeJson)
    {
        using var root = ParseKind(proposedChangeJson, ReviseProceduralRuleKind);
        var parsed = new ReviseProceduralRuleProposalV1
        {
            BasisRuleId = root.RootElement.GetProperty("basisRuleId").GetInt64(),
            RuleContent = root.RootElement.TryGetProperty("ruleContent", out var rc) ? rc.GetString() ?? "" : "",
            Source = root.RootElement.TryGetProperty("source", out var s) ? s.GetString() ?? "" : "",
        };
        if (parsed.BasisRuleId <= 0 || string.IsNullOrWhiteSpace(parsed.RuleContent) || string.IsNullOrWhiteSpace(parsed.Source))
        {
            throw new MemoryDomainException("ReviseProceduralRule proposal requires basisRuleId, ruleContent and source.");
        }

        return parsed;
    }

    public static NewProceduralRuleMemoryProposalV1 ParseNewProceduralRule(string? proposedChangeJson)
    {
        if (string.IsNullOrWhiteSpace(proposedChangeJson))
        {
            throw new MemoryDomainException("ProposedChangeJson is required for a procedural rule proposal.");
        }

        try
        {
            using var doc = JsonDocument.Parse(proposedChangeJson);
            var root = doc.RootElement;
            var kind = root.TryGetProperty("kind", out var k) ? k.GetString() : null;
            if (!string.Equals(kind, NewProceduralRuleKind, StringComparison.OrdinalIgnoreCase))
            {
                throw new MemoryDomainException("Unsupported proposal kind in ProposedChangeJson.");
            }

            var wf = root.TryGetProperty("workflowType", out var wfe) && wfe.ValueKind != JsonValueKind.Null
                ? wfe.GetString()
                : null;
            var rn = root.TryGetProperty("ruleName", out var rne) && rne.ValueKind != JsonValueKind.Null
                ? rne.GetString()
                : null;
            var content = root.TryGetProperty("ruleContent", out var ce) ? ce.GetString() : null;
            var priority = root.TryGetProperty("priority", out var pe) && pe.TryGetInt32(out var pi)
                ? pi
                : 0;
            var source = root.TryGetProperty("source", out var se) ? se.GetString() : null;
            var auth = root.TryGetProperty("authorityWeight", out var ae) && ae.TryGetDouble(out var ad)
                ? ad
                : 0.55d;
            long? basis = null;
            if (root.TryGetProperty("basisRuleId", out var be) && be.ValueKind != JsonValueKind.Null &&
                be.TryGetInt64(out var bid))
            {
                basis = bid;
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new MemoryDomainException("Procedural rule proposal requires ruleContent.");
            }

            if (basis is null)
            {
                if (string.IsNullOrWhiteSpace(wf) || string.IsNullOrWhiteSpace(rn))
                {
                    throw new MemoryDomainException(
                        "Procedural rule proposal requires workflowType and ruleName unless basisRuleId is set.");
                }

                if (string.IsNullOrWhiteSpace(source))
                {
                    throw new MemoryDomainException("Procedural rule proposal requires source.");
                }
            }
            else if (string.IsNullOrWhiteSpace(source))
            {
                throw new MemoryDomainException("Procedural rule proposal requires source.");
            }

            return new NewProceduralRuleMemoryProposalV1
            {
                WorkflowType = wf,
                RuleName = rn,
                RuleContent = content!.Trim(),
                Priority = priority,
                Source = (source ?? "").Trim(),
                AuthorityWeight = auth,
                BasisRuleId = basis,
            };
        }
        catch (JsonException ex)
        {
            throw new MemoryDomainException($"Invalid proposal JSON: {ex.Message}");
        }
    }

    private static JsonDocument ParseKind(string? proposedChangeJson, string expectedKind)
    {
        if (string.IsNullOrWhiteSpace(proposedChangeJson))
        {
            throw new MemoryDomainException("ProposedChangeJson is required.");
        }

        try
        {
            var doc = JsonDocument.Parse(proposedChangeJson);
            var kind = doc.RootElement.TryGetProperty("kind", out var k) ? k.GetString() : null;
            if (!string.Equals(kind, expectedKind, StringComparison.OrdinalIgnoreCase))
            {
                doc.Dispose();
                throw new MemoryDomainException("Unsupported proposal kind in ProposedChangeJson.");
            }

            return doc;
        }
        catch (JsonException ex)
        {
            throw new MemoryDomainException($"Invalid proposal JSON: {ex.Message}");
        }
    }

    private sealed record NewSemanticEnvelope(
        [property: JsonPropertyName("kind")] string Kind,
        [property: JsonPropertyName("key")] string Key,
        [property: JsonPropertyName("claim")] string Claim,
        [property: JsonPropertyName("domain")] string? Domain,
        [property: JsonPropertyName("initialConfidence")] double InitialConfidence);

    private sealed record NewProceduralRuleEnvelope(
        [property: JsonPropertyName("kind")] string Kind,
        [property: JsonPropertyName("workflowType")] string? WorkflowType,
        [property: JsonPropertyName("ruleName")] string? RuleName,
        [property: JsonPropertyName("ruleContent")] string RuleContent,
        [property: JsonPropertyName("priority")] int Priority,
        [property: JsonPropertyName("source")] string Source,
        [property: JsonPropertyName("authorityWeight")] double AuthorityWeight,
        [property: JsonPropertyName("basisRuleId")] long? BasisRuleId);

    private sealed record ContradictionDetectedEnvelope(
        [property: JsonPropertyName("kind")] string Kind,
        [property: JsonPropertyName("semanticMemoryId")] long SemanticMemoryId,
        [property: JsonPropertyName("key")] string Key,
        [property: JsonPropertyName("claim")] string Claim,
        [property: JsonPropertyName("confidence")] double Confidence,
        [property: JsonPropertyName("supportScore")] double SupportScore,
        [property: JsonPropertyName("contradictionScore")] double ContradictionScore);

    private sealed record ArchiveStaleSemanticEnvelope(
        [property: JsonPropertyName("kind")] string Kind,
        [property: JsonPropertyName("semanticMemoryId")] long SemanticMemoryId,
        [property: JsonPropertyName("key")] string Key,
        [property: JsonPropertyName("claim")] string Claim,
        [property: JsonPropertyName("currentConfidence")] double CurrentConfidence,
        [property: JsonPropertyName("lastSupportedAt")] DateTimeOffset? LastSupportedAt);

    private sealed record MergeSemanticCandidatesEnvelope(
        [property: JsonPropertyName("kind")] string Kind,
        [property: JsonPropertyName("sourceSemanticIds")] IReadOnlyList<long> SourceSemanticIds,
        [property: JsonPropertyName("canonicalSemanticId")] long CanonicalSemanticId,
        [property: JsonPropertyName("resultingClaim")] string ResultingClaim,
        [property: JsonPropertyName("domain")] string? Domain);

    private sealed record ConflictWithExplicitProfileEnvelope(
        [property: JsonPropertyName("kind")] string Kind,
        [property: JsonPropertyName("semanticMemoryId")] long SemanticMemoryId,
        [property: JsonPropertyName("key")] string Key,
        [property: JsonPropertyName("claim")] string Claim,
        [property: JsonPropertyName("explicitKind")] string ExplicitKind,
        [property: JsonPropertyName("explicitText")] string ExplicitText);

    private sealed record SupersedeSemanticEnvelope(
        [property: JsonPropertyName("kind")] string Kind,
        [property: JsonPropertyName("supersededSemanticId")] long SupersededSemanticId,
        [property: JsonPropertyName("canonicalSemanticId")] long CanonicalSemanticId,
        [property: JsonPropertyName("reason")] string? Reason);

    private sealed record ReviseSemanticClaimEnvelope(
        [property: JsonPropertyName("kind")] string Kind,
        [property: JsonPropertyName("semanticMemoryId")] long SemanticMemoryId,
        [property: JsonPropertyName("newClaim")] string NewClaim,
        [property: JsonPropertyName("newDomain")] string? NewDomain,
        [property: JsonPropertyName("newConfidence")] double? NewConfidence);

    private sealed record ReviseProceduralRuleEnvelope(
        [property: JsonPropertyName("kind")] string Kind,
        [property: JsonPropertyName("basisRuleId")] long BasisRuleId,
        [property: JsonPropertyName("ruleContent")] string RuleContent,
        [property: JsonPropertyName("source")] string Source);
}
