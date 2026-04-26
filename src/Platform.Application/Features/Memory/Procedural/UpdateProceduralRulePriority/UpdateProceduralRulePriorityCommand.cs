namespace Platform.Application.Features.Memory.Procedural.UpdateProceduralRulePriority;

public sealed record UpdateProceduralRulePriorityCommand(long Id, int UserId, int Priority);
