using FluentValidation;
using Platform.Application.Abstractions.Memory.Events;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory.ValueObjects;

namespace Platform.Application.Features.Memory.Events.IngestEvent;

public sealed class IngestMemoryEventCommandHandler(
    IValidator<IngestMemoryEventCommand> validator,
    IMemoryEventWriter events)
{
    public async Task<MemoryEventCreatedV1Dto> HandleAsync(
        IngestMemoryEventCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken).ConfigureAwait(false);

        var systemNow = DateTimeOffset.UtcNow;
        var occurred = command.OccurredAt ?? systemNow;
        var ev = UncommittedMemoryEvent.CreateForIngest(
            command.ResolvedUserId,
            command.EventType,
            command.Domain,
            command.WorkflowId,
            command.ProjectId,
            command.PayloadJson,
            occurred,
            systemNow);

        var result = await events.WriteAsync(ev, cancellationToken).ConfigureAwait(false);
        return new MemoryEventCreatedV1Dto(result.Id, result.OccurredAt, result.CreatedAt);
    }
}
