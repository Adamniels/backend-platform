using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Abstractions.Memory.Events;

public interface IMemoryEventsReadRepository
{
    Task<IReadOnlyList<MemoryEvent>> ListOccurredInRangeAsync(
        int userId,
        DateTimeOffset startInclusive,
        DateTimeOffset endExclusive,
        int maxTake,
        CancellationToken cancellationToken = default);
}
