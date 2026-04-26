using Platform.Domain.Features.Memory;

namespace Platform.Domain.Features.Memory.Entities;

/// <summary>One execution of the nightly (or ad-hoc) memory consolidation job for observability and idempotency.</summary>
public sealed class MemoryConsolidationRun
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public MemoryUser? User { get; set; }

    public DateTimeOffset WindowStart { get; set; }
    public DateTimeOffset WindowEnd { get; set; }
    public string IdempotencyKey { get; set; } = "";

    public int ProcessedEventsCount { get; set; }
    public int ProposalsCreatedCount { get; set; }
    public int AutoUpdatesCount { get; set; }

    public MemoryConsolidationRunStatus Status { get; set; }
    public string? Error { get; set; }

    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}
