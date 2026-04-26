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

    private sealed record NewSemanticEnvelope(
        [property: JsonPropertyName("kind")] string Kind,
        [property: JsonPropertyName("key")] string Key,
        [property: JsonPropertyName("claim")] string Claim,
        [property: JsonPropertyName("domain")] string? Domain,
        [property: JsonPropertyName("initialConfidence")] double InitialConfidence);
}
