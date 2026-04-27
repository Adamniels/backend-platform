using Platform.Application.Abstractions.Memory.Contradictions;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Infrastructure.Features.Memory.Contradictions;

public sealed class ExplicitProfileConflictDetector : IExplicitProfileConflictDetector
{
    private static readonly string[] NegativeTokens =
    [
        "not interested",
        "no longer interested",
        "avoid",
        "dislike",
        "do not prefer",
        "don't prefer",
        "stop recommending",
    ];

    public IReadOnlyList<ExplicitProfileSemanticConflict> Detect(
        ExplicitUserProfile? profile,
        IReadOnlyList<SemanticMemory> semantics)
    {
        if (profile is null || semantics.Count == 0)
        {
            return Array.Empty<ExplicitProfileSemanticConflict>();
        }

        var explicitTexts = new List<(string Kind, string Text)>();
        explicitTexts.AddRange((profile.CoreInterests ?? new()).Select(x => ("core_interest", x)));
        explicitTexts.AddRange((profile.SecondaryInterests ?? new()).Select(x => ("secondary_interest", x)));
        explicitTexts.AddRange((profile.Goals ?? new()).Select(x => ("goal", x)));
        try
        {
            explicitTexts.AddRange(
                ExplicitUserProfileContent.ParseAndValidatePreferencesJson(profile.PreferencesJson, "PreferencesJson")
                    .Select(x => ("preference", $"{x.Key}: {x.Value}")));
        }
        catch
        {
            // Invalid stored JSON should not break context assembly.
        }

        var result = new List<ExplicitProfileSemanticConflict>();
        foreach (var s in semantics)
        {
            var claim = s.Claim.ToLowerInvariant();
            if (!NegativeTokens.Any(t => claim.Contains(t, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            foreach (var ex in explicitTexts)
            {
                var text = ex.Text.Trim();
                if (text.Length < 3)
                {
                    continue;
                }

                if (claim.Contains(text.ToLowerInvariant(), StringComparison.Ordinal))
                {
                    result.Add(
                        new ExplicitProfileSemanticConflict(
                            s.Id,
                            ex.Kind,
                            text,
                            s.Claim,
                            s.Confidence,
                            s.AuthorityWeight));
                    break;
                }
            }
        }

        return result;
    }
}
