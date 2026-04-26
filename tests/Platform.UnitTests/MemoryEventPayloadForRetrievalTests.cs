using Platform.Application.Features.Memory.Events;
using Xunit;

namespace Platform.UnitTests;

public sealed class MemoryEventPayloadForRetrievalTests
{
    [Fact]
    public void TruncateForRanking_null_and_empty_return_null()
    {
        Assert.Null(MemoryEventPayloadForRetrieval.TruncateForRanking(null));
        Assert.Null(MemoryEventPayloadForRetrieval.TruncateForRanking(""));
    }

    [Fact]
    public void TruncateForRanking_short_string_unchanged()
    {
        var s = new string('a', 100);
        Assert.Equal(s, MemoryEventPayloadForRetrieval.TruncateForRanking(s));
    }

    [Fact]
    public void TruncateForRanking_long_string_capped()
    {
        var s = new string('b', MemoryEventPayloadForRetrieval.MaxCharsForRanking + 50);
        var t = MemoryEventPayloadForRetrieval.TruncateForRanking(s);
        Assert.Equal(MemoryEventPayloadForRetrieval.MaxCharsForRanking, t!.Length);
    }
}
