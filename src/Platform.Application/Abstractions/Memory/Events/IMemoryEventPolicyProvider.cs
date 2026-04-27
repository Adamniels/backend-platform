using Platform.Domain.Features.Memory;

namespace Platform.Application.Abstractions.Memory.Events;

public interface IMemoryEventPolicyProvider
{
    MemoryEventPolicy Classify(string eventType);
}

public sealed record MemoryEventPolicy(
    string Family,
    MemoryEventReliabilityClass ReliabilityClass,
    MemoryEventPrivacyClass PrivacyClass,
    bool InferenceEligible,
    bool AutoReinforceEligible,
    double DefaultReliabilityWeight,
    MemoryEvidenceSourceKind DefaultSourceKind);
