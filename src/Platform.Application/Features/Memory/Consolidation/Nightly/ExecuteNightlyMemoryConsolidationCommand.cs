namespace Platform.Application.Features.Memory.Consolidation.Nightly;

public sealed record ExecuteNightlyMemoryConsolidationCommand(
    int UserId,
    DateOnly WindowEndExclusiveUtc,
    string IdempotencyKey);
