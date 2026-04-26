using Platform.Domain.Features.Memory.Entities;
using Platform.Domain.Features.Memory.ValueObjects;

namespace Platform.Application.Abstractions.Memory.Events;

/// <summary>
/// Append-only path for <see cref="MemoryEvent" />. Implementations must insert rows only (no update/delete of <c>memory_events</c>).
/// </summary>
public interface IMemoryEventWriter
{
    Task<MemoryEventAppendResult> WriteAsync(
        UncommittedMemoryEvent ev,
        CancellationToken cancellationToken = default);
}
