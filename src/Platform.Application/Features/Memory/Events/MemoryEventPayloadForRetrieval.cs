namespace Platform.Application.Features.Memory.Events;

/// <summary>Limits how much raw payload participates in retrieval scoring (privacy + performance).</summary>
public static class MemoryEventPayloadForRetrieval
{
    /// <summary>Only this many leading characters of JSON are tokenized for relevance ranking.</summary>
    public const int MaxCharsForRanking = 512;

    public static string? TruncateForRanking(string? payloadJson)
    {
        if (string.IsNullOrEmpty(payloadJson))
        {
            return null;
        }

        return payloadJson.Length <= MaxCharsForRanking
            ? payloadJson
            : payloadJson[..MaxCharsForRanking];
    }
}
