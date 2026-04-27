using Platform.Application.Abstractions.Memory.Events;
using Platform.Application.Abstractions.Memory.Users;
using Platform.Contracts.V1.Memory;

namespace Platform.Application.Features.Memory.Events.ListMemoryEvents;

public sealed class ListMemoryEventsQueryHandler(
    IMemoryEventsReadRepository events,
    IMemoryUserContextResolver userResolver)
{
    private const int MaxPayloadPreview = 180;

    public async Task<IReadOnlyList<MemoryEventV1ListItem>> HandleAsync(
        ListMemoryEventsQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = userResolver.Resolve(query.UserId);
        var rows = await events
            .ListRecentForUserAsync(userId, query.Take, cancellationToken)
            .ConfigureAwait(false);
        return rows
            .Select(
                e => new MemoryEventV1ListItem
                {
                    Id = e.Id,
                    EventType = e.EventType,
                    Domain = e.Domain,
                    ProjectId = e.ProjectId,
                    WorkflowId = e.WorkflowId,
                    OccurredAt = e.OccurredAt,
                    PayloadPreview = TruncateJson(e.PayloadJson),
                })
            .ToList();
    }

    private static string? TruncateJson(string? json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        var t = json.Trim();
        if (t.Length <= MaxPayloadPreview)
        {
            return t;
        }

        return t.Substring(0, MaxPayloadPreview) + "…";
    }
}
