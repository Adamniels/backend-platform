using Platform.Domain.Features.Memory.ValueObjects;

namespace Platform.Application.Abstractions.Memory.Events;

/// <summary>Append path for episodic <see cref="MemoryEvent"/> (persistence in a later PR).</summary>
public interface IMemoryEventWriter
{
    Task WriteAsync(UncommittedMemoryEvent ev, CancellationToken cancellationToken = default);
}
