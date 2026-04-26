using Platform.Application.Abstractions.Memory.Events;
using Platform.Domain.Features.Memory.ValueObjects;

namespace Platform.Infrastructure.Features.Memory.Stubs;

public sealed class NoOpMemoryEventWriter : IMemoryEventWriter
{
    public Task WriteAsync(UncommittedMemoryEvent ev, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
