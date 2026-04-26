using Platform.Domain.Features.Memory.ValueObjects;

namespace Platform.Application.Abstractions.Memory.Events;

/// <summary>
/// Port for <b>append-only</b> episodic <see cref="MemoryEvent"/> emission (see master: agents emit events, platform owns persistence).
/// Infrastructure maps <see cref="UncommittedMemoryEvent"/> to a row and enforces no updates on the event store when policy is enabled.
/// </summary>
public interface IMemoryEventWriter
{
    Task WriteAsync(UncommittedMemoryEvent ev, CancellationToken cancellationToken = default);
}
