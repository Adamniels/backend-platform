using Platform.Application.Abstractions.Memory.Events;
using Platform.Domain.Features.Memory.ValueObjects;

namespace Platform.Application.Features.Memory.Events.IngestEvent;

public sealed class IngestMemoryEventCommandHandler(IMemoryEventWriter events)
{
    public async Task HandleAsync(
        IngestMemoryEventCommand command,
        CancellationToken cancellationToken = default)
    {
        var ev = new UncommittedMemoryEvent(
            command.ResolvedUserId,
            command.EventType,
            command.Domain,
            command.WorkflowId,
            command.ProjectId,
            command.PayloadJson,
            DateTimeOffset.UtcNow);

        await events.WriteAsync(ev, cancellationToken).ConfigureAwait(false);
    }
}
