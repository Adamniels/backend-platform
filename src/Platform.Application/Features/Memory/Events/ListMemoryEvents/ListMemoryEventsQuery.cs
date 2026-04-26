namespace Platform.Application.Features.Memory.Events.ListMemoryEvents;

public readonly record struct ListMemoryEventsQuery(int UserId, int Take);
