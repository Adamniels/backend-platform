namespace Platform.Contracts.V1.Memory;

public sealed class ExecuteNightlyMemoryConsolidationV1Request
{
    /// <summary>Omit to use <c>MemoryWorker:PrimaryUserId</c>.</summary>
    public int? UserId { get; set; }

    /// <summary>UTC calendar day that **ends** the window (exclusive). Default: today’s UTC date (so yesterday is processed).</summary>
    public DateOnly? WindowEndExclusiveUtc { get; set; }

    /// <summary>Override default <c>nightly-{user}-{window}</c> for tests.</summary>
    public string? IdempotencyKey { get; set; }
}

public sealed class NightlyMemoryConsolidationV1Response
{
    public long RunId { get; set; }
    public string IdempotencyKey { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTimeOffset WindowStart { get; set; }
    public DateTimeOffset WindowEnd { get; set; }
    public int ProcessedEventsCount { get; set; }
    public int ProposalsCreatedCount { get; set; }
    public int AutoUpdatesCount { get; set; }
    public int RecomputedSemanticsCount { get; set; }
    public int StaleProposalsCreatedCount { get; set; }
    public int ContradictionProposalsCreatedCount { get; set; }
    public int MergeProposalsCreatedCount { get; set; }
    public string? Error { get; set; }
    public bool FromCache { get; set; }
}
