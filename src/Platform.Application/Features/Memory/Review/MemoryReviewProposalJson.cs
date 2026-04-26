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
}
