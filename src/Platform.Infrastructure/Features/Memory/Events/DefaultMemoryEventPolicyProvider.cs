using Platform.Application.Abstractions.Memory.Events;
using Platform.Domain.Features.Memory;

namespace Platform.Infrastructure.Features.Memory.Events;

public sealed class DefaultMemoryEventPolicyProvider : IMemoryEventPolicyProvider
{
    public MemoryEventPolicy Classify(string eventType)
    {
        var t = (eventType ?? "").Trim();
        if (StartsWithAny(t, "profile.", "explicit.", "preference.", "goal.", "identity."))
        {
            return new(
                "explicit-profile",
                MemoryEventReliabilityClass.High,
                MemoryEventPrivacyClass.Sensitive,
                InferenceEligible: true,
                AutoReinforceEligible: false,
                DefaultReliabilityWeight: 0.92d,
                DefaultSourceKind: MemoryEvidenceSourceKind.UserAction);
        }

        if (StartsWithAny(t, "document.", "import.", "doc."))
        {
            return new(
                "document",
                MemoryEventReliabilityClass.Medium,
                MemoryEventPrivacyClass.General,
                InferenceEligible: true,
                AutoReinforceEligible: false,
                DefaultReliabilityWeight: 0.62d,
                DefaultSourceKind: MemoryEvidenceSourceKind.ImportedDocument);
        }

        if (StartsWithAny(t, "llm.", "extract.", "summary."))
        {
            return new(
                "llm-extraction",
                MemoryEventReliabilityClass.Low,
                MemoryEventPrivacyClass.General,
                InferenceEligible: true,
                AutoReinforceEligible: false,
                DefaultReliabilityWeight: 0.46d,
                DefaultSourceKind: MemoryEvidenceSourceKind.LlmExtraction);
        }

        if (StartsWithAny(t, "workflow.", "integration.", "learning.", "recommendation."))
        {
            return new(
                "workflow",
                MemoryEventReliabilityClass.Medium,
                MemoryEventPrivacyClass.General,
                InferenceEligible: true,
                AutoReinforceEligible: true,
                DefaultReliabilityWeight: 0.68d,
                DefaultSourceKind: MemoryEvidenceSourceKind.Workflow);
        }

        return new(
            "general",
            MemoryEventReliabilityClass.Medium,
            MemoryEventPrivacyClass.General,
            InferenceEligible: true,
            AutoReinforceEligible: true,
            DefaultReliabilityWeight: 0.55d,
            DefaultSourceKind: MemoryEvidenceSourceKind.SystemHeuristic);
    }

    private static bool StartsWithAny(string value, params string[] prefixes) =>
        prefixes.Any(p => value.StartsWith(p, StringComparison.OrdinalIgnoreCase));
}
