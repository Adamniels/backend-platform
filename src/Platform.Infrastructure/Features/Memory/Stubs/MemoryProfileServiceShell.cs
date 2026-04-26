using Platform.Application.Abstractions.Memory.Profile;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Infrastructure.Features.Memory.Stubs;

public sealed class MemoryProfileServiceShell : IMemoryProfileService
{
    public Task<IReadOnlyList<MemoryItem>> GetProfileItemsAsync(
        int _,
        CancellationToken __ = default) =>
        Task.FromResult<IReadOnlyList<MemoryItem>>(
            Array.Empty<MemoryItem>());
}
