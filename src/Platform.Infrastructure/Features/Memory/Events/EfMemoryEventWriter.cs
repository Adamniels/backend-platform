using Platform.Application.Abstractions.Memory.Events;
using Platform.Domain.Features.Memory.Entities;
using Platform.Domain.Features.Memory.ValueObjects;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.Memory.Events;

public sealed class EfMemoryEventWriter(PlatformDbContext db) : IMemoryEventWriter
{
    public async Task<MemoryEventAppendResult> WriteAsync(
        UncommittedMemoryEvent ev,
        CancellationToken cancellationToken = default)
    {
        var createdAt = DateTimeOffset.UtcNow;
        var entity = MemoryEvent.Create(
            ev.UserId,
            ev.EventType,
            ev.Domain,
            ev.WorkflowId,
            ev.ProjectId,
            ev.PayloadJson,
            ev.OccurredAt,
            createdAt);
        db.MemoryEvents.Add(entity);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new MemoryEventAppendResult(entity.Id, entity.OccurredAt, entity.CreatedAt);
    }
}
