using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Abstractions.Memory.Profile;

/// <summary>Explicit profile memory: structured user truths (highest authority). Backed by <see cref="MemoryItem"/> with <see cref="MemoryItemType.ProfileFact"/>.</summary>
public interface IMemoryProfileService
{
    Task<IReadOnlyList<MemoryItem>> GetProfileItemsAsync(
        int userId,
        CancellationToken cancellationToken = default);
}
